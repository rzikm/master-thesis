using System;
using System.Runtime.InteropServices;
using OpenSSLSandbox.Interop;

namespace OpenSSLSandbox
{
    public struct SslMethod
    {
        public SslMethod Null => new SslMethod();

        [DllImport(Libraries.Ssl)]
        private static extern SslMethod TLS_method();

        public static SslMethod Tls => TLS_method();

        private readonly IntPtr handle;

        public override string ToString()
        {
            return handle.ToString("x");
        }
    }
}