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
            buffer = new byte[16 * 1024];
            this.output = output;
            // this is safe as tests within the same class will not run parallel to each other.
            Console.SetOut(new XUnitTextWriter(output));
        }

        [Fact]
        public void TestTrivialConnection()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 1010);
            
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
            var written = client.SendData(buffer, out _);
            
            output.WriteLine("\nServer:");
            server.ReceiveData(buffer, written, ipEndPoint);
            Assert.Equal(SslError.WantRead, server.DoHandshake());
            written = server.SendData(buffer, out _);
            
            output.WriteLine("\nClient:");
            client.ReceiveData(buffer, written, ipEndPoint);
            Assert.Equal(SslError.None, client.DoHandshake());
            written = client.SendData(buffer, out _);
            
            output.WriteLine("\nServer:");
            server.ReceiveData(buffer, written, ipEndPoint);
            Assert.Equal(SslError.None, server.DoHandshake());

            Assert.True(server.Connected);
            Assert.True(client.Connected);
            var serverParams2 = client.GetPeerTransportParameters();
        }
    }
}