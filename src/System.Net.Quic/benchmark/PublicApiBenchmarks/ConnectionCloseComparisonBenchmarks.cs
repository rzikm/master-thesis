using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PublicApiBenchmarks
{
    public class ConnectionCloseComparisonBenchmarks : SslStreamComparisonBenchmark
    {
        protected sealed override Task QuicStreamServer(QuicConnection connection)
        {
            return Task.CompletedTask;
        }

        protected sealed override Task SslStreamServer(SslStream stream)
        {
            return Task.CompletedTask;
        }

        protected sealed override void IterationSetupSslStream()
        {
            TcpClient = new TcpClient();
            TcpClient.Connect((IPEndPoint) TcpListener.LocalEndpoint);
            ClientSslStream = CreateSslStream(TcpClient.GetStream());
            ClientSslStream.AuthenticateAsClient("localhost");
        }

        [Benchmark(Baseline = true)]
        public void SslStream()
        {
            ClientSslStream.Dispose();
            TcpClient.Dispose();
        }

        protected sealed override void IterationSetupQuicStream()
        {
            QuicClient = QuicFactory.CreateClient(QuicListener.ListenEndPoint);
            QuicClient.ConnectAsync().AsTask().GetAwaiter().GetResult();
        }

        [Benchmark(Description = nameof(QuicConnection))]
        public void QuicStream()
        {
            QuicClient.Dispose();
        }
    }
}