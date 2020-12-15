using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    public static class ServerListener
    {
        public static (IPEndPoint listenEp, Task finished) StartQuic(IPEndPoint endpoint, string certPath, string keyPath,
            CancellationToken cancellationToken)
        {
            TaskCompletionSource completionSource = new TaskCompletionSource();
            
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

            var listener = new QuicListener(options);
            listener.Start();
            
            List<Task> currentConnectionTasks = new List<Task>();

            Helpers.Dispatch(async () =>
            {
                while (true)
                {
                    var connection = await listener.AcceptConnectionAsync().ConfigureAwait(false);
                    Helpers.Trace("New connection");
                    var task = Helpers.Dispatch(() => ServerConnectionTask(connection, cancellationToken));
                    currentConnectionTasks.Add(task);
                }
            });

            cancellationToken.Register(() =>
                Task.WhenAll(currentConnectionTasks).ContinueWith(_ => completionSource.SetResult()));
            
            return (listener.ListenEndPoint, completionSource.Task);
        }

        public static (IPEndPoint listenEp, Task finished) StartTcpTls(IPEndPoint endpoint, string certPath, string keyPath,
            CancellationToken cancellationToken)
        {
            TaskCompletionSource completionSource = new TaskCompletionSource();
            
            var cert = Helpers.LoadCertificate(certPath, keyPath);
            TcpListener listener = new TcpListener(endpoint.Address, endpoint.Port);
            listener.Start();
            
            List<Task> currentConnectionTasks = new List<Task>();

            Helpers.Dispatch(async () =>
            {
                while (true)
                {
                    var connection = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    Helpers.Trace("New connection");
                    var task = Helpers.Dispatch(() => ServerConnectionTask(connection, cert, cancellationToken));
                    currentConnectionTasks.Add(task);
                }
            });

            cancellationToken.Register(() =>
                Task.WhenAll(currentConnectionTasks).ContinueWith(_ => completionSource.SetResult()));
            
            return ((IPEndPoint)listener.LocalEndpoint, completionSource.Task);
        }

        private static async Task ServerConnectionTask(TcpClient connection, X509Certificate2 cert,
            CancellationToken cancellationToken)
        {
            try
            {
                await using var stream = new SslStream(connection.GetStream());
                await stream.AuthenticateAsServerAsync(cert).ConfigureAwait(false);

                await ServerStreamTask(stream).ConfigureAwait(false);
            }
            catch (IOException)
            {
                // connection forcibly closed by the client, do nothing
            }
            finally
            {
                connection.Dispose();
            }
        }

        private static async Task ServerConnectionTask(QuicConnection connection, CancellationToken cancellationToken)
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

                    _ = Helpers.Dispatch(() => ServerQuicStreamTask(connection, stream));
                }
            }
            catch (QuicConnectionAbortedException e) when (e.ErrorCode == 0)
            {
                // ignore successful closing
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

        private static async Task ServerQuicStreamTask(QuicConnection connection, QuicStream stream)
        {
            try
            {
                if (!await ServerStreamTask(stream).ConfigureAwait(false))
                {
                    await connection.CloseAsync(1).ConfigureAwait(false);
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
                await stream.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static async Task<bool> ServerStreamTask(Stream stream)
        {
            var recvBuffer = new byte[32 * 1024];
            byte[] sendBuffer = null;

            var messageSize = 0;
            while (true)
            {
                var count = await stream.ReadAsync(recvBuffer).ConfigureAwait(false);
                if (count == 0)
                    // EOF: shut down
                    break;

                // process all received messages
                int offset = 0;
                while (true)
                {
                    var index = Array.IndexOf<byte>(recvBuffer, 0, offset, count - offset);
                    if (index < 0)
                    {
                        // not yet received entirely
                        messageSize += count - offset;
                        break;
                    }

                    messageSize += index + 1 - offset;
                    if (sendBuffer == null)
                    {
                        sendBuffer = Helpers.CreateMessageBuffer(messageSize);
                    }
                    else
                    {
                        if (messageSize != sendBuffer.Length)
                        {
                            return false;
                        }
                    }

                    // send a reply message back
                    await stream.WriteAsync(sendBuffer).ConfigureAwait(false);

                    offset = index + 1;
                    messageSize = 0;
                }
                
                // flush all sent messages
                await stream.FlushAsync().ConfigureAwait(false);
            }

            return true;
        }
    }
}