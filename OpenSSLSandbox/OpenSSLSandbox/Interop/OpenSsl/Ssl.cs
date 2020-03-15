using System;
using System.Runtime.InteropServices;

namespace OpenSSLSandbox.Interop.OpenSsl
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Ssl
    {
        private static readonly int managedInterfaceIndex =
            GetExNewIndex(Native.CRYPTO_EX_INDEX_SSL, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        public static Ssl Null => new Ssl();

        private readonly IntPtr handle;

        public string Version =>
            Marshal.PtrToStringUTF8(new IntPtr(Native.SSL_get_version(handle)));

        public TlsVersion MinProtoVersion
        {
            get => (TlsVersion) Ctrl(SslCtrlCommand.GetMinProtoVersion, 0, IntPtr.Zero);
            set => Ctrl(SslCtrlCommand.SetMinProtoVersion, (long) value, IntPtr.Zero);
        }

        public TlsVersion MaxProtoVersion
        {
            get => (TlsVersion) Ctrl(SslCtrlCommand.GetMaxProtoVersion, 0, IntPtr.Zero);
            set => Ctrl(SslCtrlCommand.SetMaxProtoVersion, (long) value, IntPtr.Zero);
        }

        private Ssl(SslContext ctx)
        {
            handle = Native.SSL_new(ctx);
        }

        public static Ssl New(SslContext ctx)
        {
            return new Ssl(ctx);
        }

        public static void Free(Ssl ssl)
        {
            Native.SSL_free(ssl.handle);
        }

        public int UseCertificateFile(string file, SslFiletype type)
        {
            return Native.SSL_use_certificate_file(handle, file, type);
        }

        public int UsePrivateKeyFile(string file, SslFiletype type)
        {
            return Native.SSL_use_PrivateKey_file(handle, file, type);
        }

        public int SetQuicMethod(ref QuicMethods methods)
        {
            return Native.SSL_set_quic_method(handle, ref methods);
        }

        public int SetAcceptState()
        {
            return Native.SSL_set_accept_state(handle);
        }

        public int SetConnectState()
        {
            return Native.SSL_set_connect_state(handle);
        }

        public int DoHandshake()
        {
            return Native.SSL_do_handshake(handle);
        }

        public int Ctrl(SslCtrlCommand cmd, long larg, IntPtr parg)
        {
            return Native.SSL_ctrl(handle, cmd, larg, parg);
        }

        public SslError GetError(int code)
        {
            return (SslError) Native.SSL_get_error(handle, code);
        }

        public int ProvideQuicData(SslEncryptionLevel level, ReadOnlySpan<byte> data)
        {
            fixed (byte* pData = data)
            {
                return Native.SSL_provide_quic_data(handle, level, pData, new IntPtr(data.Length));
            }
        }

        public int SetExData(int idx, IntPtr data)
        {
            return Native.SSL_set_ex_data(handle, idx, data);
        }

        public IntPtr GetExData(int idx)
        {
            return Native.SSL_get_ex_data(handle, idx);
        }

        public void SetCallbackInterface(IntPtr address)
        {
            SetExData(managedInterfaceIndex, address);
        }

        public IntPtr GetCallbackInterface()
        {
            return GetExData(managedInterfaceIndex);
        }

        public int SetTlsexHostName(string hostname)
        {
            var addr = Marshal.StringToHGlobalAnsi(hostname);
            var res = Ctrl(SslCtrlCommand.SetTlsextHostname, 0, addr);
            Marshal.FreeHGlobal(addr);
            return res;
        }

        public static int GetExNewIndex(long argl, IntPtr argp, IntPtr newFunc, IntPtr dupFunc,
            IntPtr freeFunc)
        {
            return OpenSsl.CRYPTO_get_ex_new_index(Native.CRYPTO_EX_INDEX_SSL, argl, argp, newFunc, dupFunc, freeFunc);
        }

        public override string ToString()
        {
            return handle.ToString();
        }

        private static class Native
        {
            public const int CRYPTO_EX_INDEX_SSL = 0;

            [DllImport(Libraries.Ssl)]
            public static extern IntPtr SSL_new(SslContext ctx);

            [DllImport(Libraries.Ssl)]
            public static extern void SSL_free(IntPtr ssl);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_use_certificate_file(IntPtr ssl, [MarshalAs(UnmanagedType.LPStr
                )]
                string file, SslFiletype type);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_use_PrivateKey_file(IntPtr ssl, [MarshalAs(UnmanagedType.LPStr
                )]
                string file, SslFiletype type);

            [DllImport(Libraries.Ssl)]
            public static extern byte* SSL_get_version(IntPtr ssl);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_set_quic_method(IntPtr ssl, ref QuicMethods methods);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_set_accept_state(IntPtr ssl);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_set_connect_state(IntPtr ssl);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_do_handshake(IntPtr ssl);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_ctrl(IntPtr ssl, SslCtrlCommand cmd, long larg, IntPtr parg);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_get_error(IntPtr ssl, int code);

            [DllImport(Libraries.Ssl)]
            public static extern int
                SSL_provide_quic_data(IntPtr ssl, SslEncryptionLevel level, byte* data, IntPtr len);

            [DllImport(Libraries.Ssl)]
            public static extern int SSL_set_ex_data(IntPtr ssl, int idx, IntPtr data);

            [DllImport(Libraries.Ssl)]
            public static extern IntPtr SSL_get_ex_data(IntPtr ssl, int idx);
        }
    }
}