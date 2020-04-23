using System;
using System.IO;
using System.Net;
using System.Net.Quic.Public;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;

namespace PublicApiBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.Monitoring, targetCount: 5)]
    [Config(typeof(Config))]
    public class TcpComparisonBenchmarks
    {
        class Config : ManualConfig
        {
            public Config()
            {
                Options |= ConfigOptions.DisableOptimizationsValidator;
            }
        }
        
        [Params(1024 * 1024, 32 * 1024 * 1024/*, 128 * 1024 * 1024*/)]
        public int DataLength { get; set; }

        public int SendBufferSize { get; set; } = 16 * 1024;
        public int RecvBufferSize { get; set; } = 16 * 1024;

        private QuicListener _quicListener;
        private QuicConnection _quicClient;

        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        
        private Task _serverTask;
        private Channel<int> _connectionSignalChannel;
        
        private byte[] _recvBuffer;
        private byte[] _sendBuffer;

        private async Task QuicServer()
        {
            var reader = _connectionSignalChannel.Reader;
            while (await reader.WaitToReadAsync())
            {
                await reader.ReadAsync();
                using var connection = await _quicListener.AcceptConnectionAsync();
                await using var stream = connection.OpenUnidirectionalStream();
                
                await SendData(stream);
                
                await stream.ShutdownWriteCompleted();
            }
        }
        
        private async Task TcpServer()
        {
            var reader = _connectionSignalChannel.Reader;
            while (await reader.WaitToReadAsync())
            {
                await reader.ReadAsync();
                using var client = await _tcpListener.AcceptTcpClientAsync();
                await using var stream = client.GetStream();
                await SendData(stream);
            }
        }

        private async Task SendData(Stream stream)
        {
            int written = 0;
            while (written < DataLength)
            {
                await stream.WriteAsync(_sendBuffer);
                written += _sendBuffer.Length;
            }
            
            await stream.FlushAsync();
        }

        private async Task RecvData(Stream stream)
        {
            int received = 0;
            while (received < DataLength)
            {
                received += await stream.ReadAsync(_recvBuffer);
            }
        }

        private void GlobalSetupShared()
        {
            _connectionSignalChannel = Channel.CreateUnbounded<int>();
            _sendBuffer = new byte[SendBufferSize];
            _recvBuffer = new byte[RecvBufferSize];
        }

        [GlobalSetup(Target = nameof(Quic))]
        public void GlobalSetupQuic()
        {
            GlobalSetupShared();
            _quicListener = QuicFactory.CreateListener();
            _quicListener.Start();
            _serverTask = Task.Run(QuicServer);
        }

        [GlobalSetup(Target = nameof(Tcp))]
        public void GlobalSetupTcp()
        {
            GlobalSetupShared();
            _tcpListener = new TcpListener(IPAddress.Any, 0);
            _tcpListener.Start();
            _serverTask = Task.Run(TcpServer);
        }

        [IterationSetup(Target = nameof(Quic))]
        public void IterationSetupQuic()
        {
            _connectionSignalChannel.Writer.TryWrite(0);
            _quicClient = QuicFactory.CreateClient(_quicListener.ListenEndPoint);
            _quicClient.ConnectAsync().AsTask().GetAwaiter().GetResult();
        }

        [IterationSetup(Target = nameof(Tcp))]
        public void IterationSetupTcp()
        {
            _connectionSignalChannel.Writer.TryWrite(0);
            _tcpClient = new TcpClient();
            _tcpClient.Connect((IPEndPoint) _tcpListener.LocalEndpoint);
        }

        [Benchmark(Baseline = true)]
        public async Task Tcp()
        {
            await using var stream = _tcpClient.GetStream();

            await RecvData(stream);
            
            _tcpClient.Close();
        }

        [Benchmark]
        public async Task Quic()
        {
            await using var stream = await _quicClient.AcceptStreamAsync();

            await RecvData(stream);

            // issue one more read which should block until we are sure there are no more data.
            await stream.ReadAsync(_recvBuffer);
        }

        [IterationCleanup(Target = nameof(Quic))]
        public void IterationCleanupQuic()
        {
            _quicClient.Dispose();
        }
        
        [IterationCleanup(Target = nameof(Tcp))]
        public void IterationCleanupTcp()
        {
            _tcpClient.Dispose();
        }

        [GlobalCleanup(Target = nameof(Quic))]
        public void GlobalCleanupQuic()
        {
            _connectionSignalChannel.Writer.Complete();
            _serverTask.Wait();
            _quicListener.Dispose();
        }
        
        [GlobalCleanup(Target = nameof(Tcp))]
        public void GlobalCleanupTcp()
        {
            _connectionSignalChannel.Writer.Complete();
            _serverTask.Wait();
            _tcpListener.Stop();
        }
    }
}