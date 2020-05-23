using System;
using System.Net.Security;
using System.Runtime.Serialization;

namespace DevSandbox.QuicTracer
{
    internal abstract class QuicEvent
    {
        [DataMember] public string Connection { get; set; }
        [DataMember] public DateTime Timestamp { get; set; }
    }

    [DataContract]
    internal class NewConnectionEvent : QuicEvent
    {
        [DataMember] public byte[] SourceConnectionId { get; set; }
        [DataMember] public byte[] DestinationConnectionId { get; set; }
    }

    [DataContract]
    internal class SetEncryptionSecretsEvent : QuicEvent
    {
        [DataMember] public byte[] ReadSecret { get; set; }
        [DataMember] public byte[] WriteSecret { get; set; }
        [DataMember] public TlsCipherSuite CipherSuite { get; set; }

        public enum EncryptionLevel
        {
            Initial,
            Handshake,
            Application, // keep in sync with actual names
        }

        [DataMember] public EncryptionLevel Level { get; set; }
    }

    [DataContract]
    internal class DatagramSentEvent : QuicEvent
    {
        [DataMember] public byte[] Datagram { get; set; }
    }

    [DataContract]
    internal class DatagramRecvEvent : QuicEvent
    {
        [DataMember] public byte[] Datagram { get; set; }
    }
}