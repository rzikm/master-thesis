using System;
using System.Collections.Generic;
using System.Net.Quic.Implementations.Managed.Internal;
using System.Net.Quic.Implementations.Managed.Internal.Crypto;
using System.Net.Quic.Tests.Harness;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DevSandbox.QuicTracer
{
    internal class QuicPacketLogger
    {
        private readonly ChannelReader<QuicEvent> _eventReader;

        private readonly Dictionary<string, ConnectionContext> _connections =
            new Dictionary<string, ConnectionContext>();

        public QuicPacketLogger(ChannelReader<QuicEvent> eventReader)
        {
            _eventReader = eventReader;
        }

        public async Task Start()
        {
            await foreach (var e in _eventReader.ReadAllAsync().ConfigureAwait(false))
            {
                if (!_connections.TryGetValue(e.Connection, out var connection))
                    _connections[e.Connection] = connection = new ConnectionContext(e.Connection);

                connection.OnEvent(e);
            }
        }

        private class ConnectionContext
        {
            public readonly List<QuicEvent> _delayedEvents = new List<QuicEvent>();

            public readonly EventHarnessCtx _localCtx = new EventHarnessCtx();
            public readonly EventHarnessCtx _remoteCtx = new EventHarnessCtx();
            public bool _isServer = true;
            public int _secretsCount;

            public ConnectionContext(string connection)
            {
                Connection = connection;
            }

            public string Connection { get; }

            private void ProcessQuicEvent(QuicEvent e)
            {
                switch (e)
                {
                    case DatagramRecvEvent datagramRecvEvent:
                        OnDatagramRecv(datagramRecvEvent);
                        break;
                    case DatagramSentEvent datagramSentEvent:
                        OnDatagramSent(datagramSentEvent);
                        break;
                    case NewConnectionEvent newConnectionEvent:
                        OnNewConnection(newConnectionEvent);
                        break;
                    case SetEncryptionSecretsEvent setEncryptionSecretsEvent:
                        OnSetEncryptionSecrets(setEncryptionSecretsEvent);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(e));
                }
            }

            private void OnSetEncryptionSecrets(SetEncryptionSecretsEvent e)
            {
                var readSeal = CryptoSeal.Create(e.CipherSuite, e.ReadSecret);
                var writeSeal = CryptoSeal.Create(e.CipherSuite, e.WriteSecret);

                _localCtx.AddEncryptionSecrets(readSeal, writeSeal);
                _remoteCtx.AddEncryptionSecrets(writeSeal, readSeal);
            }

            private void OnDatagramRecv(DatagramRecvEvent e)
            {
                var packets = PacketBase.ParseMany(e.Datagram, _localCtx);

                Console.WriteLine($"[{e.Timestamp:ss.ffff}] {Connection} Recv: ");
                foreach (var packet in packets)
                {
                    var i = packet.PacketType switch
                    {
                        PacketType.Initial => 0,
                        PacketType.Handshake => 1,
                        PacketType.OneRtt => 2,
                        PacketType.ZeroRtt => 2,
                        _ => throw new Exception("Not expected")
                    };
                    _localCtx._packetNumbers[i] = Math.Max(_localCtx._packetNumbers[i], packet.PacketNumber);

                    if (!_isServer &&
                        packet.PacketType == PacketType.Initial &&
                        _remoteCtx.ConnectionIdCollection.Find(((InitialPacket) packet).SourceConnectionId) == null)
                        _remoteCtx.ConnectionIdCollection.Add(
                            new ConnectionId(((InitialPacket) packet).SourceConnectionId, 1,
                                StatelessResetToken.Random()));

                    if (_isServer && packet.PacketType == PacketType.Initial &&
                        _localCtx.ConnectionIdCollection.FindBySequenceNumber(0) == null)
                    {
                        _localCtx.ConnectionIdCollection.Add(new ConnectionId(packet.DestinationConnectionId, 0,
                            StatelessResetToken.Random()));
                        _remoteCtx.ConnectionIdCollection.Add(new ConnectionId(
                            ((InitialPacket) packet).SourceConnectionId, 0, StatelessResetToken.Random()));
                    }

                    Console.WriteLine(packet);
                }

                Console.WriteLine();
            }

            private void OnDatagramSent(DatagramSentEvent e)
            {
                var packets = PacketBase.ParseMany(e.Datagram, _remoteCtx);

                Console.WriteLine($"[{e.Timestamp:ss.ffff}] {Connection} Sent: ");
                foreach (var packet in packets)
                {
                    var i = packet.PacketType switch
                    {
                        PacketType.Initial => 0,
                        PacketType.Handshake => 1,
                        PacketType.OneRtt => 2,
                        PacketType.ZeroRtt => 2,
                        _ => throw new Exception("Not expected")
                    };
                    _remoteCtx._packetNumbers[i] = Math.Max(_remoteCtx._packetNumbers[i], packet.PacketNumber);

                    Console.WriteLine(packet);
                }

                Console.WriteLine();
            }

            private void OnNewConnection(NewConnectionEvent e)
            {
                _isServer = false;
                _remoteCtx.ConnectionIdCollection.Add(new ConnectionId(e.DestinationConnectionId, 0,
                    StatelessResetToken.Random()));
                _localCtx.ConnectionIdCollection.Add(new ConnectionId(e.SourceConnectionId, 0,
                    StatelessResetToken.Random()));
            }

            public void OnEvent(QuicEvent e)
            {
                if (e is SetEncryptionSecretsEvent)
                {
                    ProcessQuicEvent(e);
                    _secretsCount++;

                    foreach (var delayedEvent in _delayedEvents) ProcessQuicEvent(delayedEvent);

                    _delayedEvents.Clear();
                }
                else if (_secretsCount < 3)
                {
                    _delayedEvents.Add(e);
                }
                else
                {
                    ProcessQuicEvent(e);
                }
            }
        }

        private class EventHarnessCtx : ITestHarnessContext
        {
            public readonly long[] _packetNumbers = {0, 0, 0};

            private readonly List<(CryptoSeal read, CryptoSeal write)> _seals =
                new List<(CryptoSeal read, CryptoSeal write)>();

            public CryptoSeal GetRecvSeal(PacketType packetType)
            {
                return _seals[GetPacketSpaceIndex(packetType)].read;
            }

            public CryptoSeal GetSendSeal(PacketType packetType)
            {
                return _seals[GetPacketSpaceIndex(packetType)].write;
            }

            public ConnectionIdCollection ConnectionIdCollection { get; } = new ConnectionIdCollection();

            public long GetPacketNumber(PacketType packetType)
            {
                return _packetNumbers[GetPacketSpaceIndex(packetType)];
            }

            public void AddEncryptionSecrets(CryptoSeal read, CryptoSeal write)
            {
                _seals.Add((read, write));
            }

            private int GetPacketSpaceIndex(PacketType packetType)
            {
                return packetType switch
                {
                    PacketType.Initial => 0,
                    PacketType.Handshake => 1,
                    PacketType.OneRtt => 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(packetType), packetType, null)
                };
            }
        }
    }
}