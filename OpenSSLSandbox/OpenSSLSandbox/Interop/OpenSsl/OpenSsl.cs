using System;
using System.Runtime.InteropServices;
using System.Text;
using OpenSSLSandbox.Interop;

namespace OpenSSLSandbox
{
    public static unsafe class OpenSsl
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ErrorPrintCallback(byte* str, UIntPtr len, IntPtr u);

        static OpenSsl()
        {
            ERR_print_errors_cb(PrintErrors, IntPtr.Zero);
        }


        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport(Libraries.Ssl)]
        public static extern SslContext SSL_CTX_new(SslMethod method);

        [DllImport(Libraries.Ssl)]
        public static extern void SSL_CTX_free(SslContext ctx);

        [DllImport(Libraries.Ssl)]
        public static extern SslMethod TLS_method();

        [DllImport(Libraries.Crypto)]
        internal static extern int CRYPTO_get_ex_new_index(int classIndex, long argl, IntPtr argp, IntPtr newFunc,
            IntPtr dupFunc, IntPtr freeFunc);

        [DllImport(Libraries.Crypto)]
        public static extern int ERR_print_errors_cb(ErrorPrintCallback callback, IntPtr u);

        private static int PrintErrors(byte* str, UIntPtr len, IntPtr _)
        {
            var span = new Span<byte>(str, (int) len.ToUInt32());
            Console.WriteLine(Encoding.ASCII.GetString(span));
            return 1;
        }
    }
}