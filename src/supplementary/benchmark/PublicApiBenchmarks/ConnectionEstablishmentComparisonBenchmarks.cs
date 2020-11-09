using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PublicApiBenchmarks
{
    public class ConnectionEstablishmentComparisonBenchmarks : SslStreamComparisonBenchmark
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
        }

        [Benchmark(Baseline = true)]
        public void SslStream()
        {
            TcpClient.Connect((IPEndPoint) TcpListener.LocalEndpoint);
            ClientSslStream = CreateSslStream(TcpClient.GetStream());
            ClientSslStream.AuthenticateAsClient("localhost");
        }

        protected sealed override void IterationCleanupSslStream()
        {
            ClientSslStream.Dispose();
            TcpClient.Dispose();
        }

        protected sealed override void IterationSetupQuicStream()
        {
            QuicClient = QuicFactory.CreateClient(QuicListener.ListenEndPoint);
        }

        [Benchmark(Description = nameof(QuicConnection))]
        public void QuicStream()
        {
            QuicClient.ConnectAsync().AsTask().GetAwaiter().GetResult();
        }

        protected sealed override void IterationCleanupQuicStream()
        {
            QuicClient.Dispose();
        }
    }
}