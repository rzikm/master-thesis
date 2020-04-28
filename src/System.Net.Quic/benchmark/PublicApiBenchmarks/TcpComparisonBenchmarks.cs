using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PublicApiBenchmarks
{
    public class StreamPerformanceComparisonBenchmarks : SslStreamComparisonBenchmark
    {
        [Params(64 * 1024, 1024 * 1024, 32 * 1024 * 1024)]
        // [Params(1024 * 1024, 32 * 1024 * 1024)]
        // [Params(32 * 1024)]
        public int DataLength { get; set; }

        // [Params(1024, 8 * 1024)]
        public int SendBufferSize { get; set; } = 16 * 1024;

        // [Params(1024, 8 * 1024)]
        public int RecvBufferSize { get; set; } = 16 * 1024;

        private byte[] _recvBuffer;
        private byte[] _sendBuffer;

        protected override async Task QuicStreamServer()
        {
            await foreach (var _ in ConnectionSignalChannel.Reader.ReadAllAsync())
            {
                var connection = await QuicListener.AcceptConnectionAsync();
                await using var stream = connection.OpenUnidirectionalStream();
                await SendData(stream);

                await stream.ShutdownWriteCompleted();

                connection.Dispose();
            }
        }

        protected override async Task SslStreamServer()
        {
            var reader = ConnectionSignalChannel.Reader;
            while (await reader.WaitToReadAsync())
            {
                await reader.ReadAsync();
                using var client = await TcpListener.AcceptTcpClientAsync();
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

        private async Task RecvData(Stream stream)
        {
            int recv;
            int total = 0;
            do
            {
                recv = await stream.ReadAsync(_recvBuffer);
                total += recv;
            } while (recv > 0);

            if (total != DataLength)
            {
                throw new InvalidOperationException("Received data size does not match send size");
            }
        }

        protected override void GlobalSetupShared()
        {
            _sendBuffer = new byte[SendBufferSize];
            _recvBuffer = new byte[RecvBufferSize];
        }

        protected override void IterationSetupQuicStream()
        {
            QuicClient = QuicFactory.CreateClient(QuicListener.ListenEndPoint);
            QuicClient.ConnectAsync().AsTask().GetAwaiter().GetResult();
        }

        [Benchmark]
        public async Task QuicStream()
        {
            await using var stream = await QuicClient.AcceptStreamAsync();
            await RecvData(stream);
        }

        protected override void IterationCleanupQuicStream()
        {
            QuicClient.Dispose();
        }

        protected override void IterationSetupSslStream()
        {
            TcpClient = new TcpClient();
            TcpClient.Connect((IPEndPoint) TcpListener.LocalEndpoint);
            ClientSslStream = new SslStream(TcpClient.GetStream(), false, (sender, certificate, chain, errors) => true);
            ClientSslStream.AuthenticateAsClient("localhost");
        }
        
        [Benchmark(Baseline = true)]
        public async Task SslStream()
        {
            await RecvData(ClientSslStream);
        }

        protected override void IterationCleanupSslStream()
        {
            TcpClient.Dispose();
        }
    }
}