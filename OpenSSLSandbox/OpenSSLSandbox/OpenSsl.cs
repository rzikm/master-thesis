using System;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenSSLSandbox
{
    public static unsafe class OpenSsl
    {
        private static int managedInterfaceIndex;

        static OpenSsl()
        {
            var res = OPENSSL_init_ssl(
                OpenSslInitFlags.LoadSslStrings |
                OpenSslInitFlags.LoadCryptoStrings,
                IntPtr.Zero);
            Console.WriteLine($"Initializing OpenSSL: {res}");

            managedInterfaceIndex = SSL_get_ex_new_index(CRYPTO_EX_INDEX_SSL, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            ERR_print_errors_cb(new ErrorPrintCallback(PrintErrors), IntPtr.Zero);
        }

        // TODO: sort out 32-64bit madness
        // private const string SslDllName = @"C:\Program Files (x86)\OpenSSL\bin\libssl-1_1.dll";
        // private const string CryptoDllName = @"C:\Program Files (x86)\OpenSSL\bin\libcrypto-1_1.dll";
        private const string LibPrefix = @"Lib\win-x86\";

        private const string SslDllName = LibPrefix + "libssl-1_1.dll";
        private const string CryptoDllName = LibPrefix + "libcrypto-1_1.dll";

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport(SslDllName)]
        public static extern int OPENSSL_init_ssl(OpenSslInitFlags opts, IntPtr ptr);

        [DllImport(SslDllName)]
        public static extern SslContext SSL_CTX_new(SslMethod method);

        [DllImport(SslDllName)]
        public static extern int SSL_use_certificate_file(Ssl ssl, [MarshalAs(UnmanagedType.LPStr
        )] string file, SslFiletype type);

        [DllImport(SslDllName)]
        public static extern int SSL_use_PrivateKey_file(Ssl ssl, [MarshalAs(UnmanagedType.LPStr
        )] string file, SslFiletype type);

        [DllImport(SslDllName)]
        public static extern byte* SSL_get_version(Ssl ssl);

        [DllImport(SslDllName)]
        public static extern void SSL_CTX_free(SslContext ctx);

        [DllImport(SslDllName)]
        public static extern int SSL_set_quic_method(Ssl ssl, ref QuicMethods methods);

        [DllImport(SslDllName)]
        public static extern Ssl SSL_new(SslContext ctx);

        [DllImport(SslDllName)]
        public static extern void SSL_free(Ssl ssl);

        [DllImport(SslDllName)]
        public static extern SslMethod TLS_method();

        [DllImport(SslDllName)]
        public static extern int SSL_set_ex_data(Ssl ssl, int idx, IntPtr data);

        [DllImport(SslDllName)]
        public static extern IntPtr SSL_get_ex_data(Ssl ssl, int idx);

        private const int CRYPTO_EX_INDEX_SSL = 0;
        public static int SSL_get_ex_new_index(long argl, IntPtr argp, IntPtr newFunc, IntPtr dupFunc,
            IntPtr freeFunc) =>
            CRYPTO_get_ex_new_index(CRYPTO_EX_INDEX_SSL, argl, argp, newFunc, dupFunc, freeFunc);

        [DllImport(CryptoDllName)]
        private static extern int CRYPTO_get_ex_new_index(int classIndex, long argl, IntPtr argp, IntPtr newFunc, IntPtr dupFunc, IntPtr freeFunc);

        public static int SSL_set_tlsext_host_name(Ssl ssl, string hostname)
        {
            var addr = Marshal.StringToHGlobalAnsi(hostname);
            var res = SSL_ctrl(ssl, SslCtrlCommand.SetTlsextHostname, 0, addr);
            Marshal.FreeHGlobal(addr);
            return res;
        }

        [DllImport(SslDllName)]
        public static extern int SSL_set_accept_state(Ssl ssl);

        [DllImport(SslDllName)]
        public static extern int SSL_set_connect_state(Ssl ssl);

        [DllImport(SslDllName)]
        public static extern int SSL_do_handshake(Ssl ssl);

        public static int SSL_set_min_proto_version(Ssl ssl, TlsVersion version)
        {
            return SSL_ctrl(ssl, SslCtrlCommand.SetMinProtoVersion, (long) version, IntPtr.Zero);
        }

        public static TlsVersion SSL_get_min_proto_version(Ssl ssl)
        {
            return (TlsVersion) SSL_ctrl(ssl, SslCtrlCommand.GetMinProtoVersion, 0, IntPtr.Zero);
        }

        public static int SSL_set_max_proto_version(Ssl ssl, TlsVersion version)
        {
            return SSL_ctrl(ssl, SslCtrlCommand.SetMaxProtoVersion, (long) version, IntPtr.Zero);
        }

        public static TlsVersion SSL_get_max_proto_version(Ssl ssl)
        {
            return (TlsVersion) SSL_ctrl(ssl, SslCtrlCommand.GetMaxProtoVersion, 0, IntPtr.Zero);
        }


        [DllImport(SslDllName)]
        public static extern int SSL_ctrl(Ssl ssl, SslCtrlCommand cmd, long larg, IntPtr parg);

        [DllImport(SslDllName)]
        public static extern int SSL_get_error(Ssl ssl, int code);

        [DllImport(SslDllName)]
        public static extern int SSL_provide_quic_data(Ssl ssl, SslEncryptionLevel level, byte* data, IntPtr len);

        public static void SetCallbackInterface(Ssl ssl, IntPtr address)
        {
            SSL_set_ex_data(ssl, managedInterfaceIndex, address);
        }

        public static IntPtr GetCallbackInterface(Ssl ssl)
        {
            return SSL_get_ex_data(ssl, managedInterfaceIndex);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ErrorPrintCallback(byte* str, UIntPtr len, IntPtr u);

        [DllImport(CryptoDllName)]
        public static extern int ERR_print_errors_cb(ErrorPrintCallback callback, IntPtr u);

        private static int PrintErrors(byte* str, UIntPtr len, IntPtr _)
        {
            Span<byte> span = new Span<byte>(str, (int) len.ToUInt32());
            Console.WriteLine(Encoding.ASCII.GetString(span));
            return 1;
        }
    }

    public struct SslContext
    {
        private IntPtr handle;
        public SslContext(IntPtr handle) => this.handle = handle;
        public static implicit operator SslContext(IntPtr ptr) => new SslContext(ptr);
        public override string ToString() => handle.ToString("x");
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Ssl
    {
        private IntPtr handle;
        public Ssl(IntPtr handle) => this.handle = handle;
        public static implicit operator Ssl(IntPtr ptr) => new Ssl(ptr);
        public override string ToString() => handle.ToString("x");
    }

    public struct SslMethod
    {
        private IntPtr handle;
        public SslMethod(IntPtr handle) => this.handle = handle;
        public static implicit operator SslMethod(IntPtr ptr) => new SslMethod(ptr);
        public override string ToString() => handle.ToString("x");
    }
}