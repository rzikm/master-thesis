using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PublicApiBenchmarks
{
    public class ConnectionEstablishmentComparisonBenchmarks : SslStreamComparisonBenchmark
    {
        protected override Task QuicStreamServer(QuicConnection connection)
        {
            return Task.CompletedTask;
        }

        protected override Task SslStreamServer(SslStream stream)
        {
            return Task.CompletedTask;
        }

        [Benchmark(Baseline = true)]
        public void SslStream()
        {
            TcpClient = new TcpClient();
            TcpClient.Connect((IPEndPoint) TcpListener.LocalEndpoint);
            ClientSslStream = CreateSslStream(TcpClient.GetStream());
            ClientSslStream.AuthenticateAsClient("localhost");
            ClientSslStream.Close();
            TcpClient.Close();
        }

        [Benchmark(Description = nameof(QuicConnection))]
        public void QuicStream()
        {
            QuicClient = QuicFactory.CreateClient(QuicListener.ListenEndPoint);
            QuicClient.ConnectAsync().AsTask().GetAwaiter().GetResult();
            QuicClient.Dispose();
        }
    }
}