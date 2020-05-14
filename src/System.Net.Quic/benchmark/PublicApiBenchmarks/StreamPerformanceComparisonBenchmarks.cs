using System.IO;
using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PublicApiBenchmarks
{
    public class StreamPerformanceComparisonBenchmarks : SslStreamComparisonBenchmark
    {
        [Params(64 * 1024, 1024 * 1024, 32 * 1024 * 1024)]
        // [Params(1024 * 1024, 32 * 1024 * 1024)]
        // [Params(16 * 1024 * 1024)]
        // [Params(1024 * 1024)]
        public int DataLength { get; set; }

        // [Params(1024, 8 * 1024)]
        public int SendBufferSize { get; set; } = 16 * 1024;

        // [Params(1024, 8 * 1024)]
        public int RecvBufferSize { get; set; } = 16 * 1024;

        private byte[] _recvBuffer;
        private byte[] _sendBuffer;

        protected override async Task QuicStreamServer(QuicConnection connection)
        {
            await using QuicStream stream = connection.OpenUnidirectionalStream();
            await SendData(stream);
            stream.Shutdown();
            await stream.ShutdownWriteCompleted();
        }

        protected override async Task SslStreamServer(SslStream stream)
        {
            await SendData(stream);
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
                // throw new InvalidOperationException("Received data size does not match send size");
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

        [Benchmark(Description = "ManagedQuicStream")]
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
            ClientSslStream = CreateSslStream(TcpClient.GetStream());
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

        [Benchmark(Description = "MsQuicStream")]
        public async Task MsQuic()
        {
            await using var stream = await QuicClient.AcceptStreamAsync();
            await RecvData(stream);
        }
    }
}