using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    internal class Client
    {
        private readonly int _streams;

        private readonly QuicConnection _connection;

        private int _requestCount;

        private readonly byte[] _sendBuffer;

        public Client(int messageSize, IPEndPoint endPoint, int streams)
        {
            _streams = streams;
            _sendBuffer = Helpers.CreateMessageBuffer(messageSize);
            _connection = new QuicConnection(new QuicClientConnectionOptions
            {
                RemoteEndPoint = endPoint,
                ClientAuthenticationOptions = new SslClientAuthenticationOptions
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>
                    {
                        Helpers.AlpnProtocol
                    }
                },
                MaxBidirectionalStreams = 1024
            });
        }

        internal static Client[] StartClients(int count, int messageSize, IPEndPoint endPoint, int streams, CancellationToken cancellationToken)
        {
            var clientTasks = Enumerable.Range(0, count)
                .Select(async _ =>
                {
                    var c = new Client(messageSize, endPoint, streams);
                    await c.ConnectAsync().ConfigureAwait(false);
                    return c;
                })
                .ToArray();

            Task.WaitAll(clientTasks);

            var clients = clientTasks.Select(i => i.Result).ToArray();
            foreach (var client in clients) Helpers.Dispatch(() => client.Run(cancellationToken));

            return clients;
        }

        public ValueTask ConnectAsync()
        {
            return _connection.ConnectAsync();
        }

        public int GetRequestCount()
        {
            return Volatile.Read(ref _requestCount);
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                Enumerable.Range(0, _streams)
                    .Select(_ => Helpers.Dispatch(() => RunStream(cancellationToken)))
                ).ConfigureAwait(false);
        }

        public async Task Close()
        {
            await _connection.CloseAsync(0).ConfigureAwait(false);
            _connection.Dispose();
        }

        public async Task RunStream(CancellationToken cancellationToken)
        {
            using var stream = _connection.OpenBidirectionalStream();
            var recvBuffer = new byte[_sendBuffer.Length];

            try
            {
                while (true)
                {
                    // send a message
                    await stream.WriteAsync(_sendBuffer, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

                    // expect an echoed message back
                    var received = 0;
                    while (received < _sendBuffer.Length)
                    {
                        var read = await stream.ReadAsync(recvBuffer, cancellationToken).ConfigureAwait(false);
                        if (read == 0)
                        {
                            await _connection.CloseAsync(1, cancellationToken).ConfigureAwait(false);
                            return;
                        }

                        received += read;
                    }

                    Interlocked.Increment(ref _requestCount);
                }
            }
            catch (OperationCanceledException)
            {
            }

            await stream.DisposeAsync().ConfigureAwait(false);
        }
    }
}