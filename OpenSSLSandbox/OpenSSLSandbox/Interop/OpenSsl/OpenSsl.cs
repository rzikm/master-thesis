using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenSSLSandbox.Interop.OpenSsl
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