using System.Net.Quic.Implementations.Managed;
using System.Net.Quic.Implementations.Managed.Internal;
using System.Net.Quic.Implementations.Managed.Internal.Crypto;
using System.Net.Quic.Implementations.Managed.Internal.OpenSsl;
using Xunit;
using Xunit.Abstractions;

namespace System.Net.Quic.Tests
{
    public class OpenSslTests
    {
        private ITestOutputHelper output;

        private byte[] buffer;

        public OpenSslTests(ITestOutputHelper output)
        {
            buffer = new byte[2048];
            this.output = output;
            // this is safe as tests within the same class will not run parallel to each other.
            Console.SetOut(new XUnitTextWriter(output));
        }

        private void PipeData(ManagedQuicConnection from, ManagedQuicConnection to)
        {
            var written = from.SendData(buffer, out _);
            Assert.Equal(PacketType.Initial, HeaderHelpers.GetPacketType(buffer[0]));
            to.ReceiveData(buffer, written, new IPEndPoint(IPAddress.Any, 1010));
        }

        [Fact]
        public void TestTrivialConnection()
        {
            QuicClientConnectionOptions clientOpts = new QuicClientConnectionOptions();
            QuicListenerOptions serverOpts = new QuicListenerOptions()
            {
                CertificateFilePath = "Certs/cert.crt",
                PrivateKeyFilePath = "Certs/cert.key"
            };
            
            ManagedQuicConnection client = new ManagedQuicConnection(clientOpts);
            ManagedQuicConnection server = new ManagedQuicConnection(serverOpts);

            output.WriteLine("Client:");
            Assert.Equal(SslError.WantRead, client.DoHandshake());
            
            output.WriteLine("\nServer:");
            PipeData(client, server);
            Assert.Equal(SslError.WantRead, server.DoHandshake());
            
            output.WriteLine("\nClient:");
            foreach (var (level, data) in server.ToSend) client.OnDataReceived(level, data);
            Assert.Equal(SslError.None, client.DoHandshake());
            
            output.WriteLine("\nServer:");
            foreach (var (level, data) in client.ToSend) server.OnDataReceived(level, data);
            Assert.Equal(SslError.None, server.DoHandshake());

            var serverParams2 = client.GetPeerTransportParameters();
        }
    }
}