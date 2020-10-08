using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DevSandbox.QuicTracer;

namespace DevSandbox
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var eventListener = new QuicEventListener();
            var logger = new QuicPacketLogger(eventListener.EventReader);
            var logTask = Task.Run(logger.Start);
            // Environment.SetEnvironmentVariable("USE_MSQUIC", "1");
            // await Run();
            // eventListener.Stop();
            // await logTask;

            await Samples.SimpleClientAndServer.Run();
        }

        private const string CertFile = "Certs/cert-combined.pfx";

        public static async Task Server()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 5000);
            listener.Start();
            
            TcpClient server = await listener.AcceptTcpClientAsync();

            X509Certificate2 cert = new X509Certificate2(CertFile);
            SslServerAuthenticationOptions options = new SslServerAuthenticationOptions()
            {
                AllowRenegotiation = false,
                ApplicationProtocols = new List<SslApplicationProtocol>()
                {
                    SslApplicationProtocol.Http11
                },
                // ServerCertificate = cert,
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                {
                    return true;
                },
                ServerCertificateSelectionCallback = (sender, name) =>
                {
                    return cert;
                }
            };

            SslStream serverStream = new SslStream(server.GetStream());

            await serverStream.AuthenticateAsServerAsync(options);
            // await serverStream.AuthenticateAsServerAsync(cert);
        }

        public static async Task SslStreamStuff()
        {
            var serverTask = Server();
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, 5000);

            SslStream clientStream = new SslStream(client.GetStream());

            X509Certificate2 cert = new X509Certificate2(CertFile, "", X509KeyStorageFlags.Exportable);
            SslClientAuthenticationOptions options = new SslClientAuthenticationOptions()
            {
                AllowRenegotiation = false,
                ApplicationProtocols = new List<SslApplicationProtocol>()
                {
                    SslApplicationProtocol.Http11
                },
                TargetHost = "localhost",
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) =>
                {
                    return true;
                },
                LocalCertificateSelectionCallback = (sender, host, certificates, certificate, issuers) =>
                {
                    return cert;
                }
            };

            await clientStream.AuthenticateAsClientAsync(options);
            await serverTask;

            Console.WriteLine("Done");
        }
    }
}
