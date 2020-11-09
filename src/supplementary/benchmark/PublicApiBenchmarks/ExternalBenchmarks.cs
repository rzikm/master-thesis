using System.Buffers.Binary;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PublicApiBenchmarks
{
    [InProcess]
    public class ExternalBenchmarks
    {
        // [Params(64 * 1024, 1024 * 1024)]
        // [Params(64 * 1024, 1024 * 1024, 32 * 1024 * 1024)]
        [Params(32 * 1024, 16 * 1024)]
        public int DataLength { get; set; }

        internal const int RecvBufferSize = 16 * 1024;
        internal const int SendBufferSize = 16 * 1024;

        private byte[] _recvBuffer = new byte[RecvBufferSize];
        private byte[] _sendBuffer = new byte[SendBufferSize];

        internal const int QuicPort = 5000;
        internal const int SslPort = 5001;

        [Benchmark]
        public async Task QuicStream()
        {
            using var connection = new QuicConnection(new IPEndPoint(IPAddress.Loopback, QuicPort), new SslClientAuthenticationOptions()
            {
                TargetHost = "localhost"
            });
            await connection.ConnectAsync();
            await using var stream = connection.OpenBidirectionalStream();

            BinaryPrimitives.WriteInt32LittleEndian(_sendBuffer, DataLength);
            await stream.WriteAsync(_sendBuffer, 0, 4);
            await stream.FlushAsync();

            var read = 0;
            while (read < DataLength)
            {
                read += await stream.ReadAsync(_recvBuffer);
            }
        }

        [Benchmark(Baseline = true)]
        public async Task SslStream()
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(IPAddress.Loopback, SslPort);
            await using var stream = new SslStream(tcpClient.GetStream());
            await stream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true,
                TargetHost = "localhost"
            });

            BinaryPrimitives.WriteInt32LittleEndian(_sendBuffer, DataLength);
            await stream.WriteAsync(_sendBuffer, 0, 4);
            await stream.FlushAsync();

            var read = 0;
            while (read < DataLength)
            {
                read += await stream.ReadAsync(_recvBuffer);
            }
        }
    }
}