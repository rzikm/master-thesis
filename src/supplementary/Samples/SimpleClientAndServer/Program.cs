using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Quic;
using System.Net.Quic.Implementations;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
        /// <summary>Specifies the form of the read/write operation to use.</summary>
        public enum ReadWriteMode
        {
            /// <summary>ReadByte / WriteByte</summary>
            SyncByte,
            /// <summary>Read(Span{byte}) / Write(ReadOnlySpan{byte})</summary>
            SyncSpan,
            /// <summary>Read(byte[], int, int) / Write(byte[], int, int)</summary>
            SyncArray,
            /// <summary>ReadAsync(byte[], int, int) / WriteAsync(byte[], int, int)</summary>
            AsyncArray,
            /// <summary>ReadAsync(Memory{byte}) / WriteAsync(ReadOnlyMemory{byte})</summary>
            AsyncMemory,
            /// <summary>EndRead(BeginRead(..., null, null)) / EndWrite(BeginWrite(..., null, null))</summary>
            SyncAPM,
            /// <summary>Task.Factory.FromAsync(s.BeginRead, s.EndRead, ...) / Task.Factory.FromAsync(s.BeginWrite, s.EndWrite, ...)</summary>
            AsyncAPM
        }

    internal class StreamPair : IDisposable
    {
        public QuicStream Stream1 { get; }
        public QuicStream Stream2 { get; }

        public List<IDisposable> Disposables { get; } = new List<IDisposable>();

        public StreamPair(QuicStream stream1, QuicStream stream2)
        {
            Stream1 = stream1;
            Stream2 = stream2;
        }

        public void Dispose()
        {
            foreach (var disposable in Disposables)
            {
                disposable.Dispose();
            }
        }
    }
    
    internal class Program
    {
        /// <summary>
        /// Gets whether the stream requires Flush{Async} to be called in order to send written data to the underlying destination.
        /// </summary>
        protected static bool FlushRequiredToWriteData => true;
        
        /// <summary>
        /// Gets whether the stream guarantees that all data written to it will be flushed as part of Flush{Async}.
        /// </summary>
        protected static bool FlushGuaranteesAllDataWritten => true;
        internal static QuicListener CreateQuicListener()
        {
            return CreateQuicListener(new IPEndPoint(IPAddress.Loopback, 0));
        }
        
        internal static QuicConnection CreateQuicConnection(IPEndPoint endpoint)
        {
            return new QuicConnection(ImplementationProvider, endpoint, GetSslClientAuthenticationOptions());
        }
        
        public static SslServerAuthenticationOptions GetSslServerAuthenticationOptions()
        {
            return new SslServerAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>() { ApplicationProtocol }
            };
        }

        public static QuicImplementationProvider ImplementationProvider { get; } = QuicImplementationProviders.ManagedMockTls;
        public static bool IsSupported => ImplementationProvider.IsSupported;

        public static SslApplicationProtocol ApplicationProtocol { get; } = new SslApplicationProtocol("quictest");

        public static SslClientAuthenticationOptions GetSslClientAuthenticationOptions()
        {
            return new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new List<SslApplicationProtocol>() { ApplicationProtocol }
            };
        }

        internal static QuicListener CreateQuicListener(IPEndPoint endpoint)
        {
            QuicListener listener = new QuicListener(ImplementationProvider, new QuicListenerOptions()
            {
                ListenEndPoint = endpoint,
                ServerAuthenticationOptions = GetSslServerAuthenticationOptions(),
                CertificateFilePath = "Certs/cert.crt",
                PrivateKeyFilePath = "Certs/cert.key"
            });
            listener.Start();
            return listener;
        }
        
        
        protected static async Task<StreamPair> CreateConnectedStreamsAsync()
        {
            QuicImplementationProvider provider = ImplementationProvider;
            var protocol = new SslApplicationProtocol("quictest");

            QuicListener listener = new QuicListener(provider, new QuicListenerOptions()
            {
                ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
                ServerAuthenticationOptions = new SslServerAuthenticationOptions { ApplicationProtocols = new List<SslApplicationProtocol> { protocol } },
                CertificateFilePath = "Certs/cert.crt",
                PrivateKeyFilePath = "Certs/cert.key"
            });

            listener.Start();

            QuicConnection connection1 = null, connection2 = null;
            QuicStream stream1 = null, stream2 = null;

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    connection1 = await listener.AcceptConnectionAsync();
                    stream1 = await connection1.AcceptStreamAsync();

                    // Hack to force stream creation
                    byte[] buffer = new byte[1];
                    await stream1.ReadAsync(buffer);
                }),
                Task.Run(async () =>
                {
                    connection2 = new QuicConnection(
                        provider,
                        listener.ListenEndPoint,
                        new SslClientAuthenticationOptions() { ApplicationProtocols = new List<SslApplicationProtocol>() { protocol } });
                    await connection2.ConnectAsync();
                    stream2 = connection2.OpenBidirectionalStream();

                    // Hack to force stream creation
                    byte[] buffer = new byte[1];
                    await stream2.WriteAsync(buffer);
                    await stream2.FlushAsync();
                }));

            var result = new StreamPair(stream1, stream2);
            result.Disposables.Add(connection1);
            result.Disposables.Add(connection2);
            result.Disposables.Add(listener);

            return result;
        }
        
        
        public static async Task CopyToAsync_AllDataCopied_Large(bool useAsync) =>
            await CopyToAsync_AllDataCopied(1024 * 1024, useAsync);
        
        
        protected static (Stream writeable, Stream readable) GetReadWritePair(StreamPair streams) =>
            GetReadWritePairs(streams).First();

        protected static IEnumerable<(Stream writeable, Stream readable)> GetReadWritePairs(StreamPair streams)
        {
            var pairs = new List<(Stream, Stream)>(2);

            if (streams.Stream1.CanWrite)
            {
                pairs.Add((streams.Stream1, streams.Stream2));
            }

            if (streams.Stream2.CanWrite)
            {
                pairs.Add((streams.Stream2, streams.Stream1));
            }

            return pairs;
        }


        public static async Task CopyToAsync_AllDataCopied(int byteCount, bool useAsync)
        {
            using StreamPair streams = await CreateConnectedStreamsAsync();
            (Stream writeable, Stream readable) = GetReadWritePair(streams);

            var results = new MemoryStream();
            byte[] dataToCopy = RandomNumberGenerator.GetBytes(byteCount);

            Task copyTask;
            if (useAsync)
            {
                copyTask = readable.CopyToAsync(results);
                await writeable.WriteAsync(dataToCopy);
            }
            else
            {
                copyTask = Task.Run(() => readable.CopyTo(results));
                writeable.Write(new ReadOnlySpan<byte>(dataToCopy));
            }

            writeable.Dispose();
            await copyTask;
        }
        
        internal static async Task FlushAsync(ReadWriteMode mode, Stream stream, CancellationToken cancellationToken = default)
        {
            switch (mode)
            {
                case ReadWriteMode.SyncByte:
                case ReadWriteMode.SyncArray:
                case ReadWriteMode.SyncSpan:
                case ReadWriteMode.SyncAPM:
                    stream.Flush();
                    break;

                case ReadWriteMode.AsyncArray:
                case ReadWriteMode.AsyncMemory:
                case ReadWriteMode.AsyncAPM:
                    await stream.FlushAsync(cancellationToken);
                    break;

                default:
                    throw new Exception($"Unknown mode: {mode}");
            }
        }
        
        internal static async Task ReadWrite_Success(ReadWriteMode mode, int writeSize, bool startWithFlush)
        {
            foreach (CancellationToken nonCanceledToken in new[] { CancellationToken.None, new CancellationTokenSource().Token })
            {
                using StreamPair streams = await CreateConnectedStreamsAsync();

                foreach ((Stream writeable, Stream readable) in GetReadWritePairs(streams))
                {
                    if (startWithFlush)
                    {
                        await FlushAsync(mode, writeable, nonCanceledToken);
                    }

                    byte[] writerBytes = RandomNumberGenerator.GetBytes(writeSize);
                    var readerBytes = new byte[writerBytes.Length];

                    Task writes = Task.Run(async () =>
                    {
                        await WriteAsync(mode, writeable, writerBytes, 0, writerBytes.Length, nonCanceledToken);

                        if (FlushRequiredToWriteData)
                        {
                            if (FlushGuaranteesAllDataWritten)
                            {
                                await writeable.FlushAsync();
                            }
                            else
                            {
                                await writeable.DisposeAsync();
                            }
                        }
                    });

                    int n = 0;
                    while (n < readerBytes.Length)
                    {
                        int r = await ReadAsync(mode, readable, readerBytes, n, readerBytes.Length - n);
                        n += r;
                    }

                    await writes;

                    if (!FlushGuaranteesAllDataWritten)
                    {
                        break;
                    }
                }
            }
        }
        
        
        
        internal static async Task WriteAsync(ReadWriteMode mode, Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            switch (mode)
            {
                case ReadWriteMode.SyncByte:
                    for (int i = offset; i < offset + count; i++)
                    {
                        stream.WriteByte(buffer[i]);
                    }
                    break;

                case ReadWriteMode.SyncArray:
                    stream.Write(buffer, offset, count);
                    break;

                case ReadWriteMode.SyncSpan:
                    stream.Write(buffer.AsSpan(offset, count));
                    break;

                case ReadWriteMode.AsyncArray:
                    await stream.WriteAsync(buffer, offset, count, cancellationToken);
                    break;

                case ReadWriteMode.AsyncMemory:
                    await stream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
                    break;

                case ReadWriteMode.SyncAPM:
                    stream.EndWrite(stream.BeginWrite(buffer, offset, count, null, null));
                    break;

                case ReadWriteMode.AsyncAPM:
                    await Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, offset, count, null);
                    break;

                default:
                    throw new Exception($"Unknown mode: {mode}");
            }
        }
        
        internal static async Task<int> ReadAsync(ReadWriteMode mode, Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (mode == ReadWriteMode.SyncByte)
            {
                if (count > 0)
                {
                    int b = stream.ReadByte();
                    if (b != -1)
                    {
                        buffer[offset] = (byte)b;
                        return 1;
                    }
                }

                return 0;
            }

            return mode switch
            {
                ReadWriteMode.SyncArray => stream.Read(buffer, offset, count),
                ReadWriteMode.SyncSpan => stream.Read(buffer.AsSpan(offset, count)),
                ReadWriteMode.AsyncArray => await stream.ReadAsync(buffer, offset, count, cancellationToken),
                ReadWriteMode.AsyncMemory => await stream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken),
                ReadWriteMode.SyncAPM => stream.EndRead(stream.BeginRead(buffer, offset, count, null, null)),
                ReadWriteMode.AsyncAPM => await Task.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, offset, count, null),
                _ => throw new Exception($"Unknown mode: {mode}"),
            };
        }
        
        internal static async Task ZeroByteRead_BlocksUntilDataAvailableOrNops(ReadWriteMode mode)
        {
            using StreamPair streams = await CreateConnectedStreamsAsync();
            foreach ((Stream writeable, Stream readable) in GetReadWritePairs(streams))
            {
                for (int iter = 0; iter < 2; iter++)
                {
                    Task<int> zeroByteRead = Task.Run(() => ReadAsync(mode, readable, Array.Empty<byte>(), 0, 0));

                    if (BlocksOnZeroByteReads)
                    {
                        Task write = Task.Run(async () =>
                        {
                            await writeable.WriteAsync(Encoding.UTF8.GetBytes("hello"));
                            if (FlushRequiredToWriteData)
                            {
                                if (FlushGuaranteesAllDataWritten)
                                {
                                    await writeable.FlushAsync();
                                }
                                else
                                {
                                    await writeable.DisposeAsync();
                                }
                            }
                        });

                        var readBytes = new byte[5];
                        int count = 0;
                        while (count < readBytes.Length)
                        {
                            int n = await readable.ReadAsync(readBytes.AsMemory(count));
                            count += n;
                        }
                        
                        await write;
                    }
                    else
                    {
                        await zeroByteRead;
                    }

                    if (!FlushGuaranteesAllDataWritten)
                    {
                        return;
                    }
                }
            }
        }

        public static bool BlocksOnZeroByteReads { get; } = false;
        
        
        internal static async Task ConcurrentBidirectionalReadsWrites_Success()
        {
            using StreamPair streams = await CreateConnectedStreamsAsync();
            Stream client = streams.Stream1, server = streams.Stream2;
            if (!(client.CanRead && client.CanWrite && server.CanRead && server.CanWrite))
            {
                return;
            }

            const string Text = "This is a test.  This is only a test.";
            byte[] sendBuffer = Encoding.UTF8.GetBytes(Text);
            DateTime endTime = DateTime.UtcNow + TimeSpan.FromSeconds(2);
            Func<Stream, Stream, Task> work = async (client, server) =>
            {
                var readBuffer = new byte[sendBuffer.Length];
                while (DateTime.UtcNow < endTime)
                {
                    await Task.WhenAll(
                        Task.Run(async () =>
                            {
                                await client.WriteAsync(sendBuffer, 0, sendBuffer.Length);
                                if (FlushRequiredToWriteData)
                                {
                                    await client.FlushAsync();
                                }
                            }),
                        Task.Run(async () =>
                        {
                            int received = 0, bytesRead = 0;
                            while (received < readBuffer.Length && (bytesRead = await server.ReadAsync(readBuffer.AsMemory(received))) > 0)
                            {
                                received += bytesRead;
                            }
                        }));
                }
            };

            await Task.WhenAll(
                Task.Run(() => work(client, server)),
                Task.Run(() => work(server, client)));
        }

        private static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("DOTNETQUIC_TRACE", "console");
            await ConcurrentBidirectionalReadsWrites_Success();
        }
    }
}
