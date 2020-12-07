using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
    /// <summary>
    ///     Runs a simple and verbose sample of sending fixed amount of data from server to client.
    /// </summary>
    public class SimpleClientAndServer
    {
        private const int DataSizeBytes = 64 * 1024 * 1024;

        private static readonly IPEndPoint serverEndpoint = GetEndpoint("localhost", 5000);
        // private const int DataSizeBytes = 1024 * 1024;

        private static IPEndPoint GetEndpoint(string host, int port)
        {
            var ipHostInfo = Dns.GetHostEntry(host);
            var ipAddress = ipHostInfo.AddressList[0];
            return new IPEndPoint(ipAddress, port);
        }

        private static async Task Server(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine(@"Starting listener");
                using var listener = new QuicListener(QuicImplementationProviders.Managed, new QuicListenerOptions
                {
                    ListenEndPoint = serverEndpoint,
                    ServerAuthenticationOptions = new SslServerAuthenticationOptions
                    {
                        ApplicationProtocols = new List<SslApplicationProtocol>
                        {
                            new SslApplicationProtocol("sample")
                        }
                    },
                    CertificateFilePath = "Certs/cert.crt",
                    PrivateKeyFilePath = "Certs/cert.key"
                });

                listener.Start();

                Console.WriteLine($@"listening on {listener.ListenEndPoint}");

                Console.WriteLine("Waiting for incoming connection...");
                using var connection = await listener.AcceptConnectionAsync(cancellationToken).ConfigureAwait(false);

                Console.WriteLine("Connection accepted, opening stream");
                var stream = connection.OpenUnidirectionalStream();

                Console.WriteLine($"Writing {DataSizeBytes} bytes of data");
                var buffer = new byte[1024 * 16];

                // write known data so that we can assert it on the other size
                for (var i = 0; i < buffer.Length; i++) buffer[i] = (byte) i;

                var written = 0;

                while (written < DataSizeBytes)
                {
                    await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                    written += buffer.Length;
                }

                stream.Shutdown();
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine("Data written, closing.");

                await stream.ShutdownWriteCompleted(cancellationToken).ConfigureAwait(false);
                Console.WriteLine("Server shutdown complete");

                await stream.DisposeAsync();
                Console.WriteLine("Server stream dispose");

                connection.Dispose();
                Console.WriteLine("Server connection disposed");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception at server:");
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task Client(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Creating client connection");
                using var client = new QuicConnection(serverEndpoint, new SslClientAuthenticationOptions
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        new SslApplicationProtocol("sample")
                    },
                    TargetHost = "localhost"
                });

                Console.WriteLine($"Connecting to the server from {client.LocalEndPoint}");
                await client.ConnectAsync(cancellationToken).ConfigureAwait(false);

                Console.WriteLine("Client connected, waiting for stream.");
                var stream = await client.AcceptStreamAsync(cancellationToken).ConfigureAwait(false);

                Console.WriteLine("Stream received, pulling data");
                var buffer = new byte[1024 * 16];

                var total = 0;
                int recv;
                do
                {
                    recv = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

                    for (var i = 0; i < recv; i++)
                        if (buffer[i] != (byte) (total + i))
                            throw new InvalidOperationException("Data corrupted");

                    total += recv;
                } while (recv > 0);

                if (total > DataSizeBytes) throw new InvalidOperationException("Received unexpectedly more data");

                Console.WriteLine("Received all bytes");

                await stream.DisposeAsync().ConfigureAwait(false);
                Console.WriteLine("Client stream disposed");

                client.Dispose();
                Console.WriteLine("Client connection disposed");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception at client:");
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task Run()
        {
            var cts = new CancellationTokenSource();
            var serverTask = Task.Run(() => Server(cts.Token));
            var clientTask = Task.Run(() => Client(cts.Token));

            ConsoleCancelEventHandler cancelHandler = null;
            cancelHandler = (sender, args) =>
            {
                Console.WriteLine("Cancelling");
                cts.Cancel();
                args.Cancel = true;
                Console.CancelKeyPress -= cancelHandler;
            };
            Console.CancelKeyPress += cancelHandler;
            await Task.WhenAll(serverTask, clientTask).ConfigureAwait(false);
            Console.WriteLine("Gracefully finished");
        }
    }
}
