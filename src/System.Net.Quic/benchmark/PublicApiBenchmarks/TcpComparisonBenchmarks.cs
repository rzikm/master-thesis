using System;
using System.IO;
using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;

namespace PublicApiBenchmarks
{
    [MemoryDiagnoser]
    // [SimpleJob(RunStrategy.Monitoring, targetCount: 20)]
    [Config(typeof(Config))]
    [InProcess]
    public class SslStreamComparisonBenchmarks
    {
        private static void Log(string message)
        {
            // Console.WriteLine(message);
        }

        private const string CertFilePath = "Certs/cert.crt";
        private const string CertPrivateKeyPath = "Certs/cert.key";
        private const string CertPfx = "Certs/cert-combined.pfx";

        class Config : ManualConfig
        {
            public Config()
            {
                Options |= ConfigOptions.DisableOptimizationsValidator;
            }
        }

        // [Params(8 * 1024 * 1024, 32 * 1024 * 1024, 128 * 1024 * 1024)]
        // [Params(1024 * 1024, 32 * 1024 * 1024)]
        [Params(16 * 1024 * 1024)]
        // [Params(32 * 1024)] 
        public int DataLength { get; set; }

        // [Params(1024, 8 * 1024)]
        public int SendBufferSize { get; set; } = 16 * 1024;

        // [Params(1024, 8 * 1024)]
        public int RecvBufferSize { get; set; } = 16 * 1024;

        private QuicListener _quicListener;
        private QuicConnection _quicClient;

        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private SslStream _sslStream;

        private Task _serverTask;
        private Channel<int> _connectionSignalChannel;

        private byte[] _recvBuffer;
        private byte[] _sendBuffer;

        private async Task QuicServer()
        {
            await foreach (var _ in _connectionSignalChannel.Reader.ReadAllAsync())
            {
                Log("Server: waiting for connection.");

                var connection = await _quicListener.AcceptConnectionAsync();
                Log("Server: opening stream.");
                await using var stream = connection.OpenUnidirectionalStream();
                await SendData(stream);
                Log("Server: data sent.");

                await stream.ShutdownWriteCompleted();
                Log("Server: data confirmed delivered.");

                connection.Dispose();
                Log("Server: connection closed");
            }
        }

        private async Task SslStreamServer()
        {
            var reader = _connectionSignalChannel.Reader;
            while (await reader.WaitToReadAsync())
            {
                await reader.ReadAsync();
                using var client = await _tcpListener.AcceptTcpClientAsync();
                await using var stream = new SslStream(client.GetStream(), false);

                var cert = new X509Certificate2(CertPfx);
                await stream.AuthenticateAsServerAsync(cert);
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

        private async Task<int> RecvData(Stream stream)
        {
            int recv;
            int total = 0;
            do
            {
                recv = await stream.ReadAsync(_recvBuffer);
                total += recv;
            } while (recv > 0);

            return total;
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
            Log("Global setup");
            GlobalSetupShared();
            _quicListener = QuicFactory.CreateListener();
            _quicListener.Start();
            _serverTask = Task.Run(QuicServer);
        }

        [IterationSetup(Target = nameof(Quic))]
        public void IterationSetupQuic()
        {
            Log("iteration setup");
            _connectionSignalChannel.Writer.TryWrite(0);
            _quicClient = QuicFactory.CreateClient(_quicListener.ListenEndPoint);
            _quicClient.ConnectAsync().AsTask().GetAwaiter().GetResult();
        }

        [Benchmark]
        public async Task Quic()
        {
            Log("Benchmark method");
            await using var stream = await _quicClient.AcceptStreamAsync();

            await RecvData(stream);
        }

        [IterationCleanup(Target = nameof(Quic))]
        public void IterationCleanupQuic()
        {
            Log("Iteration cleanup");
            _quicClient.Dispose();
        }

        [GlobalCleanup(Target = nameof(Quic))]
        public void GlobalCleanupQuic()
        {
            Log("Global cleanup");
            _connectionSignalChannel.Writer.Complete();
            _serverTask.Wait();
            _quicListener.Dispose();
        }


        [GlobalSetup(Target = nameof(SslStream))]
        public void GlobalSetupSslStream()
        {
            GlobalSetupShared();
            _tcpListener = new TcpListener(IPAddress.Any, 0);
            _tcpListener.Start();
            _serverTask = Task.Run(SslStreamServer);
        }

        [IterationSetup(Target = nameof(SslStream))]
        public void IterationSetupSslStream()
        {
            _connectionSignalChannel.Writer.TryWrite(0);
            _tcpClient = new TcpClient();
            _tcpClient.Connect((IPEndPoint) _tcpListener.LocalEndpoint);
            _sslStream = new SslStream(_tcpClient.GetStream(), false, (sender, certificate, chain, errors) => true);
            _sslStream.AuthenticateAsClient("localhost");
        }


        [Benchmark(Baseline = true)]
        public async Task SslStream()
        {
            await RecvData(_sslStream);

            _tcpClient.Close();
        }

        [IterationCleanup(Target = nameof(SslStream))]
        public void IterationCleanupSslStream()
        {
            _tcpClient.Dispose();
        }

        [GlobalCleanup(Target = nameof(SslStream))]
        public void GlobalCleanupSslStream()
        {
            _connectionSignalChannel.Writer.Complete();
            _serverTask.Wait();
            _tcpListener.Stop();
        }
    }
}