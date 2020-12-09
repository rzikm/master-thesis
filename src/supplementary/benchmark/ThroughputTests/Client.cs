using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    internal abstract class Client : IDisposable
    {
        protected readonly ClientCommonOptions _options;

        private int _requestCount;

        private List<TimeSpan>[] _latencies;
        
        private readonly byte[] _sendBuffer;

        protected Client(EndPoint endPoint, ClientCommonOptions options)
        {
            _latencies = Enumerable.Range(0, Math.Max(options.Streams, 1)).Select(_ => new List<TimeSpan>()).ToArray();
            _options = options;
            _sendBuffer = Helpers.CreateMessageBuffer(options.MessageSize);
        }

        internal static Client[] StartClients(EndPoint endPoint, ClientCommonOptions options, CancellationToken cancellationToken)
        {
            var clientTasks = Enumerable.Range(0, options.Connections)
                .Select(async _ =>
                {
                    var c = options.Tcp ? (Client) new TcpTlsClient(endPoint, options) : new QuicClient(endPoint, options);
                    await c.ConnectAsync().ConfigureAwait(false);
                    return c;
                })
                .ToArray();

            Task.WaitAll(clientTasks);

            var clients = clientTasks.Select(i => i.Result).ToArray();
            foreach (var client in clients) Helpers.Dispatch(() => client.Run(cancellationToken));

            return clients;
        }

        public abstract ValueTask ConnectAsync();

        public int GetRequestCount()
        {
            return Volatile.Read(ref _requestCount);
        }

        public List<TimeSpan>[] ExchangeMeasurements(List<TimeSpan>[] empty)
        {
            Debug.Assert(empty.Length == _latencies.Length);
            return Interlocked.Exchange(ref _latencies, empty);
        }

        public abstract Task CloseAsync();

        protected async Task RunStreamContinuous(Stream stream, CancellationToken cancellationToken)
        {
            var send = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // send a message
                        await stream.WriteAsync(_sendBuffer, cancellationToken).ConfigureAwait(false);
                        Interlocked.Increment(ref _requestCount);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            });
            
            var recvBuffer = new byte[_sendBuffer.Length];
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // just drain the replies
                    await stream.ReadAsync(recvBuffer, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }

            await send;
        }

        protected async Task<bool> RunStreamSingleMessage(int index, Stream stream, CancellationToken cancellationToken)
        {
            var recvBuffer = new byte[_sendBuffer.Length];

            try
            {
                var sw = new Stopwatch();
                while (true)
                {
                    // send a message
                    await stream.WriteAsync(_sendBuffer, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    sw.Restart();

                    // expect an echoed message back
                    var received = 0;
                    while (received < _sendBuffer.Length)
                    {
                        var read = await stream.ReadAsync(recvBuffer, cancellationToken).ConfigureAwait(false);
                        if (read == 0)
                        {
                            return false;
                        }

                        received += read;
                    }

                    Interlocked.Increment(ref _requestCount);
                    Volatile.Read(ref _latencies)![index].Add(sw.Elapsed);
                }
            }
            catch (OperationCanceledException)
            {
            }

            return true;
        }

        private Task Run(CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                Enumerable.Range(0, _options.Streams)
                    .Select(i =>
                    {
                        return _options.NoWait
                            ? Helpers.Dispatch(() => RunStreamContinuous(cancellationToken))
                            : Helpers.Dispatch(() => RunStreamSingleMessage(i, cancellationToken));
                    })
            );
        }

        protected abstract Task RunStreamSingleMessage(int stream, CancellationToken cancellationToken);
        protected abstract Task RunStreamContinuous(CancellationToken cancellationToken);
        public void Dispose()
        {
            CloseAsync().GetAwaiter().GetResult();
        }
    }

    internal class TcpTlsClient : Client
    {
        private readonly IPEndPoint _endPoint;
        private TcpClient _client;
        public TcpTlsClient(EndPoint endPoint, ClientCommonOptions options) : base(endPoint, options)
        {
            _endPoint = (IPEndPoint) endPoint;
            _client = new TcpClient();
        }

        public override async ValueTask ConnectAsync()
        {
            await _client.ConnectAsync(_endPoint).ConfigureAwait(false);
        }

        public override Task CloseAsync()
        {
            _client.Close();
            return Task.CompletedTask;
        }

        private async Task<SslStream> CreateStream()
        {
            var stream = new SslStream(_client.GetStream(), false, (sender, certificate, chain, errors) => true);
            await stream.AuthenticateAsClientAsync("localhost").ConfigureAwait(false);
            return stream;
        }

        protected override async Task RunStreamSingleMessage(int index, CancellationToken cancellationToken)
        {
            await using var stream = await CreateStream();
            await RunStreamSingleMessage(index, stream, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task RunStreamContinuous(CancellationToken cancellationToken)
        {
            await using var stream = await CreateStream();
            await RunStreamContinuous(stream, cancellationToken).ConfigureAwait(false);
        }
    }

    internal class QuicClient : Client
    {
        protected QuicConnection _connection;

        public QuicClient(EndPoint endPoint, ClientCommonOptions options) : base(endPoint, options)
        {
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

        public override ValueTask ConnectAsync()
        {
            return _connection.ConnectAsync();
        }

        protected override async Task RunStreamSingleMessage(int index, CancellationToken cancellationToken)
        {
            await using var stream = _connection.OpenBidirectionalStream();
            if (!await RunStreamSingleMessage(index, stream, cancellationToken).ConfigureAwait(false))
            {
                await _connection.CloseAsync(1, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override async Task RunStreamContinuous(CancellationToken cancellationToken)
        {
            await using var stream = _connection.OpenBidirectionalStream();
            await RunStreamContinuous(stream, cancellationToken).ConfigureAwait(false);
        }

        public override async Task CloseAsync()
        {
            await _connection.CloseAsync(0).ConfigureAwait(false);
            _connection.Dispose();
        }
    }
}