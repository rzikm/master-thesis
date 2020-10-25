using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace DevSandbox
{
    public class SingleConnectionThroughputTest
    {
        private const int reportDelayMillis = 1000;

        static IPEndPoint GetEndpoint(string host, int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            return new IPEndPoint(ipAddress, port);
        }

        static IPEndPoint serverEndpoint = GetEndpoint("localhost", 6000);

        public static async Task Run()
        {
            using var listener = new QuicListener(new QuicListenerOptions()
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
            });

            CancellationTokenSource cts = new CancellationTokenSource();

            ConsoleCancelEventHandler cancelHandler = null;
            cancelHandler = (sender, args) =>
            {
                args.Cancel = true;
                cts.Cancel();
                Console.WriteLine("stopping");
                Console.CancelKeyPress -= cancelHandler;
            };
            Console.CancelKeyPress += cancelHandler;

            _ = Server(listener, cts.Token);

            try
            {
                await Client();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task Client()
        {
            using var client = new QuicConnection(serverEndpoint, new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>()
                {
                    new SslApplicationProtocol("sample")
                },
                TargetHost = "localhost"
            });

            await client.ConnectAsync();

            var stream = await client.AcceptStreamAsync().ConfigureAwait(false);

            byte[] buffer = new byte[1024 * 16];

            long total;
            int recv;
            Stopwatch sw = new Stopwatch();
            do
            {
                total = 0;
                sw.Restart();

                do
                {
                    recv = await stream.ReadAsync(buffer).ConfigureAwait(false);
                    total += recv;
                } while (recv > 0 && sw.ElapsedMilliseconds < reportDelayMillis);

                Console.WriteLine($"{total/sw.Elapsed.TotalSeconds/1024/1024:n3} MiB/s");

            } while (recv > 0);

            await client.CloseAsync(0);
        }

        public static async Task Server(QuicListener listener, CancellationToken token)
        {
            QuicConnection connection;
            listener.Start();
            
            while ((connection = await listener.AcceptConnectionAsync()) != null)
            {
                var local = connection;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ServerFunction(local, token);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    local.Dispose();
                });
            }
        }

        public static async Task ServerFunction(QuicConnection connection, CancellationToken cancellationToken)
        {
            var stream = connection.OpenUnidirectionalStream();

            byte[] buffer = new byte[1024 * 16];


            try
            {
                bool doSend = true;
                using var registration = cancellationToken.Register(() => { doSend = false; });
                
                // send until the other endpoint sends stop sending frame
                while (doSend)
                {
                    await stream.WriteAsync(buffer).ConfigureAwait(false);
                }

                stream.Shutdown();
                await stream.ShutdownWriteCompleted();
            }
            catch (QuicStreamAbortedException)
            {
            }

            stream.Shutdown();
            await stream.ShutdownWriteCompleted().ConfigureAwait(false);
            await stream.DisposeAsync();
            connection.Dispose();
        }
    }
}