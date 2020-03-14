using System;

namespace OpenSSLSandbox
{
    internal class Program
    {
        private static string SslErrorToString(int code)
        {
            switch (code)
            {
                case 0:
                    return "SSL_ERROR_NONE";
                case 1:
                    return "SSL_ERROR_SSL";
                case 2:
                    return "SSL_ERROR_WANT_READ";
                case 3:
                    return "SSL_ERROR_WANT_WRITE";
                case 4:
                    return "SSL_ERROR_WANT_X509_LOOKUP";
                case 5:
                    return "SSL_ERROR_SYSCALL";
                case 6:
                    return "SSL_ERROR_ZERO_RETURN";
                case 7:
                    return "SSL_ERROR_WANT_CONNECT";
                case 8:
                    return "SSL_ERROR_WANT_ACCEPT";
                case 9:
                    return "SSL_ERROR_WANT_ASYNC";
                case 10:
                    return "SSL_ERROR_WANT_ASYNC_JOB";
                case 11:
                    return "SSL_ERROR_WANT_CLIENT_HELLO_CB";
                default:
                    throw new ArgumentOutOfRangeException($"Unknown error code {code}");
            }
        }

        private static void CheckSslError(Ssl ssl, int code)
        {
            var e = ssl.GetError(code);
            if (e != 0) Console.WriteLine(SslErrorToString(e));
        }

        private static void Main(string[] args)
        {
            var res = 0;
            var context = OpenSsl.SSL_CTX_new(OpenSsl.TLS_method());

            var client = new Handshake(context, "localhost:4000");
            var server = new Handshake(context,
                cert: @"Certs\cert.crt",
                privateKey: @"Certs\cert.key");

            while (true)
            {
                Console.WriteLine("Client:");
                res = client.DoHandshake();
                CheckSslError(client.Ssl, res);
                foreach ((var level, var data) in client.ToSend) server.OnDataReceived(level, data);
                Console.WriteLine();

                Console.WriteLine("Server:");
                res = server.DoHandshake();
                CheckSslError(server.Ssl, res);
                foreach ((var level, var data) in server.ToSend) client.OnDataReceived(level, data);

                Console.Read();
            }
        }
    }
}