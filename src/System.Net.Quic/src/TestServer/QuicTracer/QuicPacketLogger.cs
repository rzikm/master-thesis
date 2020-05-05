using System;
using System.Collections.Generic;
using System.Net.Quic.Implementations.Managed.Internal;
using System.Net.Quic.Implementations.Managed.Internal.Crypto;
using System.Net.Quic.Tests.Harness;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestServer.QuicTracer
{
    internal class QuicPacketLogger
    {
        private readonly ChannelReader<QuicEvent> _eventReader;
        
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
        
        private readonly EventHarnessCtx _serverCtx = new EventHarnessCtx();
        private readonly EventHarnessCtx _clientCtx = new EventHarnessCtx();
        
        public QuicPacketLogger(ChannelReader<QuicEvent> eventReader)
        {
            _eventReader = eventReader;
        }

        public async void Start()
        {
            await Task.Yield();
            List<QuicEvent> delayedEvents = new List<QuicEvent>();
            int secretsCount = 0;
            
            await foreach (var e in _eventReader.ReadAllAsync())
            {
                if (e is SetEncryptionSecretsEvent)
                {
                    ProcessQuicEvent(e);
                    secretsCount++;
                    
                    foreach (var delayedEvent in delayedEvents)
                    {
                        ProcessQuicEvent(delayedEvent);
                    }
                    
                    delayedEvents.Clear();
                }
                else if (secretsCount < 3)
                {
                    delayedEvents.Add(e);
                }
                else
                {
                    ProcessQuicEvent(e);
                }
            }
        }

        private class EventHarnessCtx : ITestHarnessContext
        {
            List<(CryptoSeal read, CryptoSeal write)> _seals = new List<(CryptoSeal read, CryptoSeal write)>();
            private int[] _packetNumbers = new[] {0, 0, 0};

            public void AddEncryptionSecrets(CryptoSeal read, CryptoSeal write)
            {
                _seals.Add((read, write));
            }
            
            public CryptoSeal GetRecvSeal(PacketType packetType)
            {
                return _seals[GetPacketSpaceIndex(packetType)].read;
            }

            public CryptoSeal GetSendSeal(PacketType packetType)
            {
                return _seals[GetPacketSpaceIndex(packetType)].write;
            }

            private int GetPacketSpaceIndex(PacketType packetType)
            {
                return packetType switch
                {
                    PacketType.Initial => 0,
                    PacketType.Handshake => 1,
                    PacketType.OneRtt => 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(packetType), packetType, null),
                };
            }

            public ConnectionIdCollection ConnectionIdCollection { get; } = new ConnectionIdCollection();
            public long GetPacketNumber(PacketType packetType)
            {
                return _packetNumbers[GetPacketSpaceIndex(packetType)];
            }
        }

        private void OnSetEncryptionSecrets(SetEncryptionSecretsEvent e)
        {
            var readSeal = CryptoSeal.Create(e.CipherSuite, e.ReadSecret);
            var writeSeal = CryptoSeal.Create(e.CipherSuite, e.WriteSecret);
            
            _clientCtx.AddEncryptionSecrets(readSeal, writeSeal);
            _serverCtx.AddEncryptionSecrets(writeSeal, readSeal);
        }

        private void OnDatagramRecv(DatagramRecvEvent e)
        {
            var packets = PacketBase.ParseMany(e.Datagram, _clientCtx);

            Console.WriteLine($"[{e.Timestamp:ss.ffff}] Recv: ");
            foreach (PacketBase packet in packets)
            {
                if (packet.PacketType == PacketType.Handshake && _serverCtx.ConnectionIdCollection.FindBySequenceNumber(1) == null)
                {
                    _serverCtx.ConnectionIdCollection.Add(
                        new ConnectionId(((HandShakePacket) packet).SourceConnectionId, 1,
                            StatelessResetToken.Random()));
                }
                Console.WriteLine(packet);
            }
            
            Console.WriteLine();
        }

        private void OnDatagramSent(DatagramSentEvent e)
        {
            var packets = PacketBase.ParseMany(e.Datagram, _serverCtx);
            
            Console.WriteLine($"[{e.Timestamp:ss.ffff}] Sent: ");
            foreach (PacketBase packet in packets)
            {
                Console.WriteLine(packet);
            }
            
            Console.WriteLine();
        }

        private void OnNewConnection(NewConnectionEvent e)
        {
            _serverCtx.ConnectionIdCollection.Add(new ConnectionId(e.DestinationConnectionId, 0, StatelessResetToken.Random()));
            _clientCtx.ConnectionIdCollection.Add(new ConnectionId(e.SourceConnectionId, 0, StatelessResetToken.Random()));
        }
    }
}