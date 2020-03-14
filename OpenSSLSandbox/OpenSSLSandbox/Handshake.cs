using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenSSLSandbox
{
    [StructLayout(LayoutKind.Sequential)]
    public class Handshake : IQuicCallback, IDisposable
    {
        private readonly string _cert;
        private readonly string _privateKey;

        public Ssl Ssl { get; }

        public List<(SslEncryptionLevel, byte[])> ToSend { get; }

        public Handshake(SslContext ctx, string address = null, string cert = null, string privateKey = null)
        {
            _cert = cert;
            _privateKey = privateKey;

            var gcHandle = GCHandle.Alloc(this);
            Ssl = OpenSsl.SSL_new(ctx);
            ToSend = new List<(SslEncryptionLevel, byte[])>();

            OpenSsl.SetCallbackInterface(Ssl, GCHandle.ToIntPtr(gcHandle));
            OpenSsl.SSL_set_min_proto_version(Ssl, TlsVersion.Tls13);
            OpenSsl.SSL_set_max_proto_version(Ssl, TlsVersion.Tls13);
            OpenSsl.SSL_set_quic_method(Ssl, ref QuicMethods.Instance);

            if (cert != null)
                OpenSsl.SSL_use_certificate_file(Ssl, cert, SslFiletype.Pem);
            if (privateKey != null)
                OpenSsl.SSL_use_PrivateKey_file(Ssl, privateKey, SslFiletype.Pem);

            if (address == null)
            {
                OpenSsl.SSL_set_accept_state(Ssl);
            }
            else
            {
                OpenSsl.SSL_set_connect_state(Ssl);
                OpenSsl.SSL_set_tlsext_host_name(Ssl, address);
            }
        }

        public int SetEncryptionSecrets(SslEncryptionLevel level, byte[] readSecret, byte[] writeSecret)
        {
            Console.WriteLine($"SetEncryptionSecrets({level})");

            return 1;
        }

        public int AddHandshakeData(SslEncryptionLevel level, byte[] data)
        {
            Console.WriteLine($"AddHandshakeData({level})");
            ToSend.Add((level, data));
            return 1;
        }

        public int Flush()
        {
            Console.WriteLine("FlushFlight");

            return 1;
        }

        public int SendAlert(SslEncryptionLevel level, TlsAlert alert)
        {
            Console.WriteLine($"SendAlert({level}): {(byte)alert} (0x{(byte)alert:x2}) - {alert}");

            return 1;
        }

        public int DoHandshake() => OpenSsl.SSL_do_handshake(Ssl);

        public unsafe int OnDataReceived(SslEncryptionLevel level, byte[] data)
        {
            fixed (byte* pData = data)
            {
                return OpenSsl.SSL_provide_quic_data(Ssl, level, pData, new IntPtr(data.Length));
            }
        }

        public void Dispose()
        {
            var ptr = OpenSsl.GetCallbackInterface(Ssl);
            var gcHandle = GCHandle.FromIntPtr(ptr);

            gcHandle.Free();

            OpenSsl.SSL_free(Ssl);
        }
    }
}