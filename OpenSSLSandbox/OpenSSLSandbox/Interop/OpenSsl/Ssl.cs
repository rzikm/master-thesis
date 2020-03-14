using System;
using System.Runtime.InteropServices;
using OpenSSLSandbox.Interop;

namespace OpenSSLSandbox
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Ssl
    {
        private static readonly int managedInterfaceIndex =
            GetExNewIndex(CRYPTO_EX_INDEX_SSL, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        public static Ssl Null => new Ssl();

        private readonly IntPtr handle;

        private Ssl(SslContext ctx)
        {
            handle = SSL_new(ctx);
        }

        [DllImport(Libraries.Ssl)]
        public static extern IntPtr SSL_new(SslContext ctx);

        public static Ssl New(SslContext ctx)
        {
            return new Ssl(ctx);
        }

        [DllImport(Libraries.Ssl)]
        public static extern void SSL_free(IntPtr ssl);

        public static void Free(Ssl ssl)
        {
            SSL_free(ssl.handle);
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_use_certificate_file(IntPtr ssl, [MarshalAs(UnmanagedType.LPStr
            )]
            string file, SslFiletype type);

        public int UseCertificateFile(string file, SslFiletype type)
        {
            return SSL_use_certificate_file(handle, file, type);
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_use_PrivateKey_file(IntPtr ssl, [MarshalAs(UnmanagedType.LPStr
            )]
            string file, SslFiletype type);

        public int UsePrivateKeyFile(string file, SslFiletype type)
        {
            return SSL_use_PrivateKey_file(handle, file, type);
        }

        [DllImport(Libraries.Ssl)]
        private static extern byte* SSL_get_version(IntPtr ssl);

        public string GetVersion()
        {
            return Marshal.PtrToStringUTF8(new IntPtr(SSL_get_version(handle)));
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_set_quic_method(IntPtr ssl, ref QuicMethods methods);

        public int SetQuicMethod(ref QuicMethods methods)
        {
            return SSL_set_quic_method(handle, ref methods);
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_set_accept_state(IntPtr ssl);

        public int SetAcceptState()
        {
            return SSL_set_accept_state(handle);
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_set_connect_state(IntPtr ssl);

        public int SetConnectState()
        {
            return SSL_set_connect_state(handle);
        }

        [DllImport(Libraries.Ssl)]
        public static extern int SSL_do_handshake(IntPtr ssl);

        public int DoHandshake()
        {
            return SSL_do_handshake(handle);
        }

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

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_ctrl(IntPtr ssl, SslCtrlCommand cmd, long larg, IntPtr parg);

        public int Ctrl(SslCtrlCommand cmd, long larg, IntPtr parg)
        {
            return SSL_ctrl(handle, cmd, larg, parg);
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_get_error(IntPtr ssl, int code);

        public int GetError(int code)
        {
            return SSL_get_error(handle, code);
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_provide_quic_data(IntPtr ssl, SslEncryptionLevel level, byte* data, IntPtr len);

        public int ProvideQuicData(SslEncryptionLevel level, ReadOnlySpan<byte> data)
        {
            fixed (byte* pData = data)
            {
                return SSL_provide_quic_data(handle, level, pData, new IntPtr(data.Length));
            }
        }

        [DllImport(Libraries.Ssl)]
        private static extern int SSL_set_ex_data(IntPtr ssl, int idx, IntPtr data);

        public int SetExData(int idx, IntPtr data)
        {
            return SSL_set_ex_data(handle, idx, data);
        }

        [DllImport(Libraries.Ssl)]
        public static extern IntPtr SSL_get_ex_data(IntPtr ssl, int idx);

        public IntPtr GetExData(int idx)
        {
            return SSL_get_ex_data(handle, idx);
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

        private const int CRYPTO_EX_INDEX_SSL = 0;

        public static int GetExNewIndex(long argl, IntPtr argp, IntPtr newFunc, IntPtr dupFunc,
            IntPtr freeFunc)
        {
            return OpenSsl.CRYPTO_get_ex_new_index(CRYPTO_EX_INDEX_SSL, argl, argp, newFunc, dupFunc, freeFunc);
        }

        public override string ToString()
        {
            return handle.ToString();
        }
    }
}