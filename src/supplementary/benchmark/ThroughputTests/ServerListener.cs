using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    public static class ServerListener
    {
        public static IPEndPoint Start(IPEndPoint endpoint, string certPath, string keyPath, CancellationToken cancellationToken)
        {
            var options = new QuicListenerOptions
            {
                CertificateFilePath = certPath,
                PrivateKeyFilePath = keyPath,
                ListenEndPoint = endpoint,
                ServerAuthenticationOptions = new SslServerAuthenticationOptions
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        Helpers.AlpnProtocol
                    }
                },
                MaxBidirectionalStreams = 1024
            };

            var listener = new QuicListener(QuicImplementationProviders.Default, options);
            listener.Start();
            
            Helpers.Dispatch(async () =>
            {
                while (true)
                {
                    var connection = await listener.AcceptConnectionAsync().ConfigureAwait(false);
                    Helpers.Trace("New connection");
                    _ = Helpers.Dispatch(() => ServerConnectionTask(connection, cancellationToken));
                }
            });

            return listener.ListenEndPoint;
        }

        public static async Task ServerConnectionTask(QuicConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    var stream = await connection.AcceptStreamAsync(cancellationToken).ConfigureAwait(false);

                    if (!stream.CanRead || !stream.CanWrite)
                    {
                        await connection.CloseAsync(1, cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    _ = Helpers.Dispatch(() => ServerStreamTask(connection, stream));
                }
            }
            catch (QuicConnectionAbortedException e) when (e.ErrorCode == 0)
            {
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                await connection.CloseAsync(1).ConfigureAwait(false);
                connection.Dispose();
            }
        }

        public static async Task ServerStreamTask(QuicConnection connection, QuicStream stream)
        {
            var recvBuffer = new byte[4 * 1024];
            byte[] sendBuffer = null;

            try
            {
                var messageSize = 0;

                while (true)
                {
                    var count = await stream.ReadAsync(recvBuffer).ConfigureAwait(false);
                    if (count == 0)
                        // EOF: shut down
                        break;

                    // the message ends with 0
                    var index = Array.IndexOf<byte>(recvBuffer, 0, 0, count);
                    if (index < 0)
                    {
                        // not yet received entirely
                        messageSize += count;
                        continue;
                    }

                    messageSize += index + 1;
                    if (sendBuffer == null)
                    {
                        sendBuffer = Helpers.CreateMessageBuffer(messageSize);
                    }
                    else
                    {
                        if (messageSize != sendBuffer.Length)
                        {
                            Console.WriteLine("Wrong message size received");
                            await connection.CloseAsync(1).ConfigureAwait(false);
                        }
                    }

                    // send reply message back
                    await stream.WriteAsync(sendBuffer).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);

                    messageSize = 0;
                }
            }
            catch (QuicStreamAbortedException e) when (e.ErrorCode == 0)
            {
            }
            catch (QuicConnectionAbortedException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                await stream.DisposeAsync();
            }
        }
    }
}