using System.Collections.Generic;
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
                ServerAuthenticationOptions = new SslServerAuthenticationOptions()
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>()
                    {
                        new SslApplicationProtocol("benchmark")
                    }
                },
                ListenEndPoint = endpoint ?? new IPEndPoint(IPAddress.Loopback, 0)
            });
        }

        public static QuicConnection CreateClient(IPEndPoint serverAddress)
        {
            return new QuicConnection(serverAddress, new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>()
                {
                    new SslApplicationProtocol("benchmark")
                }
            });
        }
    }
}