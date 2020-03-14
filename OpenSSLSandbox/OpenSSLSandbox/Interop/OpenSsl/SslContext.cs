using System;
using System.Runtime.InteropServices;
using OpenSSLSandbox.Interop;

namespace OpenSSLSandbox
{
    public struct SslContext
    {
        public static SslContext Null => new SslContext();

        private readonly IntPtr handle;

        private SslContext(IntPtr handle)
        {
            this.handle = handle;
        }

        [DllImport(Libraries.Ssl)]
        public static extern IntPtr SSL_CTX_new(SslMethod method);

        public static SslContext New(SslMethod method)
        {
            return new SslContext(SSL_CTX_new(method));
        }

        [DllImport(Libraries.Ssl)]
        public static extern void SSL_CTX_free(IntPtr ctx);

        public static void Free(SslContext ctx)
        {
            SSL_CTX_free(ctx.handle);
        }

        public override string ToString()
        {
            return handle.ToString("x");
        }
    }
}