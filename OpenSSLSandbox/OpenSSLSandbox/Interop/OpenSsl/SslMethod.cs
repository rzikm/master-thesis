using System;
using System.Runtime.InteropServices;

namespace OpenSSLSandbox.Interop.OpenSsl
{
    public struct SslMethod
    {
        public SslMethod Null => new SslMethod();

        public static SslMethod Tls => Native.TLS_method();

        private readonly IntPtr handle;

        public override string ToString()
        {
            return handle.ToString("x");
        }

        private static class Native
        {
            [DllImport(Libraries.Ssl)]
            public static extern SslMethod TLS_method();
        }
    }
}