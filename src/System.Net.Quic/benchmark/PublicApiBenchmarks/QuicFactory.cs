using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;

namespace PublicApiBenchmarks
{
    public static class QuicFactory
    {
        public static QuicListener CreateListener(IPEndPoint endpoint = null)
        {
            return new QuicListener(new QuicListenerOptions()
            {
                CertificateFilePath = "Certs/cert.crt",
                PrivateKeyFilePath = "Certs/cert.key",
                ListenEndPoint = endpoint
            });
        }

        public static QuicConnection CreateClient(IPEndPoint serverAddress)
        {
            return new QuicConnection(serverAddress, new SslClientAuthenticationOptions());
        }
    }
}