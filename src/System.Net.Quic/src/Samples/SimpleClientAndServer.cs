using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading.Tasks;

namespace Samples
{
    /// <summary>
    ///     Runs a simple and verbose sample of sending fixed amount of data from server to client.
    /// </summary>
    public class SimpleClientAndServer
    {
        private const int DataSizeBytes = 1024 * 1024;

        static IPEndPoint GetEndpoint(string host, int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            return new IPEndPoint(ipAddress, port);
        }

        static IPEndPoint serverEndpoint = GetEndpoint("localhost", 5000);

        static async Task Server()
        {
            QuicListenerOptions options = new QuicListenerOptions()
            {
                ListenEndPoint = serverEndpoint,
                ServerAuthenticationOptions = new SslServerAuthenticationOptions()
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>()
                    {
                        new SslApplicationProtocol("sample")
                    },
                },
                CertificateFilePath = "Certs/cert.crt",
                PrivateKeyFilePath = "Certs/cert.key"
            };

            Console.WriteLine($@"Starting listener");

            QuicListener listener = new QuicListener(options);
            listener.Start();
            Console.WriteLine($@"listening on {listener.ListenEndPoint}");

            Console.WriteLine("Waiting for incoming connection...");
            var connection = await listener.AcceptConnectionAsync();

            Console.WriteLine("Connection accepted, opening stream");
            var stream = connection.OpenUnidirectionalStream();

            Console.WriteLine($"Writing {DataSizeBytes} bytes of data");
            byte[] buffer = new byte[1024 * 16];

            // write known data so that we can assert it on the other size
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) i;
            }

            int written = 0;

            while (written < DataSizeBytes)
            {
                await stream.WriteAsync(buffer);
                written += buffer.Length;
            }

            stream.Shutdown();
            await stream.FlushAsync();
            Console.WriteLine("Data written, closing.");

            await stream.ShutdownWriteCompleted();
            Console.WriteLine("Server shutdown complete");

            await stream.DisposeAsync();
            Console.WriteLine("Server stream dispose");

            connection.Dispose();
            Console.WriteLine("Server connection disposed");

            listener.Dispose();
            Console.WriteLine("Listener disposed");
        }

        public static async Task Client()
        {
            Console.WriteLine("Creating client connection");
            var client = new QuicConnection(serverEndpoint, new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>()
                {
                    new SslApplicationProtocol("sample")
                },
                TargetHost = "localhost"
            });
            
            Console.WriteLine("Connecting to the server");
            await client.ConnectAsync();

            Console.WriteLine("Client connected, waiting for stream.");
            var stream = await client.AcceptStreamAsync();

            Console.WriteLine("Stream received, pulling data");
            byte[] buffer = new byte[1024 * 16];

            int total = 0;
            int recv;
            do
            {
                recv = await stream.ReadAsync(buffer);

                for (int i = 0; i < recv; i++)
                {
                    if (buffer[i] != (byte) (total + i))
                    {
                        throw new InvalidOperationException("Data corrupted");
                    }
                }

                total += recv;
            } while (recv > 0);

            if (total > DataSizeBytes)
            {
                throw new InvalidOperationException("Received unexpectedly more data");
            }

            Console.WriteLine($"Received all bytes");

            await stream.DisposeAsync();
            Console.WriteLine($"Client stream disposed");

            client.Dispose();
            Console.WriteLine($"Client connection disposed");
        }

        public static async Task Run()
        {
            var serverTask = Server();
            var clientTask = Client();
            await Task.WhenAll(serverTask, clientTask);
            Console.WriteLine("Gracefully finished");
        }
    }
}