using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Echo
{
    public static class Echo
    {
        public static async Task<int> RunServer(IPEndPoint listenEp, string certificateFile, string privateKeyFile, CancellationToken token)
        {
            using QuicListener listener = new QuicListener(new QuicListenerOptions
            {
                ListenEndPoint = listenEp,
                CertificateFilePath = certificateFile,
                PrivateKeyFilePath = privateKeyFile,
                ServerAuthenticationOptions = new SslServerAuthenticationOptions
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        new SslApplicationProtocol("echo")
                    }
                }
            });
            
            // QuicListener must be started before accepting connections.
            listener.Start();
            
            // tasks that need to be awaited when trying to exit gracefully
            List<Task> tasks = new List<Task>();

            try
            {
                QuicConnection conn;
                while ((conn = await listener.AcceptConnectionAsync(token)) != null)
                {
                    // copy the connection into a variable with narrower scope so
                    // that it is not shared among multiple lambdas
                    QuicConnection captured = conn;
                    var task = Task.Run(
                        () => HandleServerConnection(captured, token));
                    tasks.Add(task);
                }
            }
            finally
            {
                // wait until all connections are closed
                await Task.WhenAll(tasks);
            }
            
            return 0;
        }

        public static async Task HandleServerConnection(QuicConnection connection, CancellationToken token)
        {
            try
            {
                QuicStream stream = await connection.AcceptStreamAsync(token);
                
                byte[] buffer = new byte[4 * 1024];

                int read;
                while ((read = await stream.ReadAsync(buffer, token)) > 0)
                {
                    // echo the read data back
                    await stream.WriteAsync(buffer, 0, read, token);
                    await stream.FlushAsync(token);
                }
            }
            finally
            {
                // gracefully close the connection with 0 error code
                await connection.CloseAsync(0);
            }
        }
        
        public static async Task<int> RunClient(IPEndPoint serverEp, CancellationToken token)
        {
            using var client = new QuicConnection(new QuicClientConnectionOptions
            {
                RemoteEndPoint = serverEp,
                ClientAuthenticationOptions = new SslClientAuthenticationOptions
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        new SslApplicationProtocol("echo") // same as used for server
                    }
                }
            });

            await client.ConnectAsync(token);

            try
            {
                await using QuicStream stream = client.OpenBidirectionalStream();
                
                // spawn a reader task to not let server be flow-control blocked
                _ = Task.Run(async () =>
                {
                    byte[] arr = new byte[4 * 1024];
                    int read;
                    while ((read = await stream.ReadAsync(arr, token)) > 0)
                    {
                        string s = Encoding.ASCII.GetString(arr, 0, read);
                        Console.WriteLine($"Received: {s}");
                    }
                });
            
                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    // convert into ASCII byte array before sending
                    byte[] bytes = Encoding.ASCII.GetBytes(line);
                    await stream.WriteAsync(bytes, token);
                    // flush the stream to send the data immediately
                    await stream.FlushAsync(token);
                }
                
                // once all stdin is written, close the stream
                stream.Shutdown();
                
                // wait until the server receives all data
                await stream.ShutdownWriteCompleted(token);
            }
            finally
            {
                await client.CloseAsync(0, token);
            }

            return 0;
        }
    }
}