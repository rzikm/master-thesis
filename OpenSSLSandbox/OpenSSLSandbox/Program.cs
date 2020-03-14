using System;
using OpenSSLSandbox.Interop.OpenSsl;

namespace OpenSSLSandbox
{
    internal class Program
    {
        private static void CheckSslError(Ssl ssl, int code)
        {
            Console.WriteLine($"SslError: {ssl.GetError(code)}");
        }

        private static void Main(string[] args)
        {
            var res = 0;
            var context = SslContext.New(SslMethod.Tls);

            var client = new Handshake(context, "localhost:4000");
            var server = new Handshake(context,
                cert: @"Certs\cert.crt",
                privateKey: @"Certs\cert.key");

            while (true)
            {
                Console.WriteLine("Client:");
                res = client.DoHandshake();
                CheckSslError(client.Ssl, res);
                foreach (var (level, data) in client.ToSend) server.OnDataReceived(level, data);
                Console.WriteLine();

                Console.WriteLine("Server:");
                res = server.DoHandshake();
                CheckSslError(server.Ssl, res);
                foreach (var (level, data) in server.ToSend) client.OnDataReceived(level, data);

                Console.Read();
            }
        }
    }
}