using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace DevSandbox
{
    internal static class BenchmarkServer
    {
        internal const int BufferSize = 16 * 1024;
        internal const int QuicPort = 5000;
        internal const int SslPort = 5001;

        public static async Task QuicServer()
        {
            using var listener = new QuicListener(new QuicListenerOptions()
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
                ListenEndPoint = new IPEndPoint(IPAddress.Any, QuicPort)
            });

            listener.Start();

            while (true)
            {
                var connection = await listener.AcceptConnectionAsync(CancellationToken.None);
                _ = Task.Run(() => QuicServer(connection));
            }
        }

        public static async Task QuicServer(QuicConnection connection)
        {
            await using var stream = await connection.AcceptStreamAsync();

            var buffer = new byte[BufferSize];

            var read = await stream.ReadAsync(buffer);
            var size = BinaryPrimitives.ReadInt32LittleEndian(buffer);

            var written = 0;
            while (written < size)
            {
                written += buffer.Length;
                await stream.WriteAsync(buffer, written == size);
            }

            await stream.FlushAsync();
            stream.Shutdown();
            await stream.ShutdownWriteCompleted();

            await connection.CloseAsync(0);
        }

        public static async Task SslServer()
        {
            var listener = new TcpListener(IPAddress.Any, SslPort);
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => SslServer(client));
            }
        }

        public static async Task SslServer(TcpClient client)
        {
            await using var stream = new SslStream(client.GetStream());
            await stream.AuthenticateAsServerAsync(new X509Certificate2("Certs/cert-combined.pfx"));

            var buffer = new byte[BufferSize];

            var read = await stream.ReadAsync(buffer);
            var size = BinaryPrimitives.ReadInt32LittleEndian(buffer);

            var written = 0;
            while (written < size)
            {
                written += buffer.Length;
                await stream.WriteAsync(buffer);
            }

            await stream.FlushAsync();

            client.Close();
        }
    }
}