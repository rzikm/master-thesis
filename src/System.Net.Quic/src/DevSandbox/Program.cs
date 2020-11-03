using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DevSandbox
{
    internal static class Program
    {
        private const string CertFile = "Certs/cert-combined.pfx";

        public static async Task Main(string[] args)
        {
            // var eventListener = new QuicEventListener();
            // var logger = new QuicPacketLogger(eventListener.EventReader);
            // var logTask = Task.Run(logger.Start);
            // Environment.SetEnvironmentVariable("USE_MSQUIC", "1");
            // await Run();
            // eventListener.Stop();
            // await logTask;

            // await Samples.SimpleClientAndServer.Run();
            // Environment.SetEnvironmentVariable("DOTNETQUIC_TRACE", "qlog");
            await SingleConnectionThroughputTest.Run();
        }

        public static async Task Server()
        {
            var listener = new TcpListener(IPAddress.Loopback, 5000);
            listener.Start();

            var server = await listener.AcceptTcpClientAsync();

            var cert = new X509Certificate2(CertFile);
            var options = new SslServerAuthenticationOptions
            {
                AllowRenegotiation = false,
                ApplicationProtocols = new List<SslApplicationProtocol>
                {
                    SslApplicationProtocol.Http11
                },
                // ServerCertificate = cert,
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; },
                ServerCertificateSelectionCallback = (sender, name) => { return cert; }
            };

            var serverStream = new SslStream(server.GetStream());

            await serverStream.AuthenticateAsServerAsync(options);
            // await serverStream.AuthenticateAsServerAsync(cert);
        }

        public static async Task SslStreamStuff()
        {
            var serverTask = Server();
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, 5000);

            var clientStream = new SslStream(client.GetStream());

            var cert = new X509Certificate2(CertFile, "", X509KeyStorageFlags.Exportable);
            var options = new SslClientAuthenticationOptions
            {
                AllowRenegotiation = false,
                ApplicationProtocols = new List<SslApplicationProtocol>
                {
                    SslApplicationProtocol.Http11
                },
                TargetHost = "localhost",
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; },
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