using System;
using System.Diagnostics.Tracing;
using System.Net.Security;
using System.Threading.Channels;

namespace TestServer.QuicTracer
{
    internal class QuicEventListener : EventListener
    {
        // event ids blatantly copied from NetEventSource.Quic
        private const int ConnectClientStart = 17;
        private const int ConnectSuccess = ConnectClientStart + 1;

        private const int PacketSentId = ConnectSuccess + 1;
        private const int PacketLostId = PacketSentId + 1;
        private const int PacketDroppedId = PacketLostId + 1;
        private const int SetEncryptionSecretsId = PacketDroppedId + 1;
        private const int DatagramSentId = SetEncryptionSecretsId + 1;
        private const int DatagramRecvId = DatagramSentId + 1;

        private EventSource _quicEventSource;

        private readonly Channel<QuicEvent> _eventChannel =
            Channel.CreateUnbounded<QuicEvent>(new UnboundedChannelOptions {SingleReader = true, SingleWriter = true});

        public ChannelReader<QuicEvent> EventReader => _eventChannel.Reader;

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Microsoft-System-Net-Quic")
            {
                _quicEventSource = eventSource;
                EnableEvents(eventSource, EventLevel.Verbose, (EventKeywords) 2);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource == _quicEventSource)
            {
                QuicEvent e = null;
                switch (eventData.EventId)
                {
                    case ConnectClientStart:
                        e = new NewConnectionEvent()
                        {
                            Connection = (string) eventData.Payload[0],
                            SourceConnectionId = (byte[]) eventData.Payload[1],
                            DestinationConnectionId = (byte[]) eventData.Payload[2],
                        };
                        break;
                    case SetEncryptionSecretsId:
                        e = new SetEncryptionSecretsEvent()
                        {
                            Connection = (string) eventData.Payload[0],
                            Level = Enum.Parse<SetEncryptionSecretsEvent.EncryptionLevel>(eventData.Payload[1]
                                .ToString()),
                            CipherSuite = (TlsCipherSuite) eventData.Payload[2],
                            ReadSecret = (byte[]) eventData.Payload[3],
                            WriteSecret = (byte[]) eventData.Payload[4]
                        };
                        break;
                    case DatagramSentId:
                        e = new DatagramSentEvent()
                        {
                            Connection = (string) eventData.Payload[0],
                            Datagram = (byte[]) eventData.Payload[1]
                        };
                        break;
                    case DatagramRecvId:
                        e = new DatagramRecvEvent()
                        {
                            Connection = (string) eventData.Payload[0],
                            Datagram = (byte[]) eventData.Payload[1]
                        };
                        break;
                }

                if (e != null)
                {
                    e.Timestamp = eventData.TimeStamp;
                    _eventChannel.Writer.TryWrite(e);
                }
            }
        }

        public void Stop()
        {
            _eventChannel.Writer.Complete();
        }
    }
}