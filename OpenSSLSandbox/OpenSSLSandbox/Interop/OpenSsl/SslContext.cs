using System;
using System.Runtime.InteropServices;

namespace OpenSSLSandbox.Interop.OpenSsl
{
    public struct SslContext
    {
        public static SslContext Null => new SslContext();

        private readonly IntPtr handle;

        private SslContext(IntPtr handle)
        {
            this.handle = handle;
        }

        public static SslContext New(SslMethod method)
        {
            return new SslContext(Native.SSL_CTX_new(method));
        }

        public static void Free(SslContext ctx)
        {
            Native.SSL_CTX_free(ctx.handle);
        }

        public override string ToString()
        {
            return handle.ToString("x");
        }

        private static class Native
        {
            [DllImport(Libraries.Ssl)]
            public static extern void SSL_CTX_free(IntPtr ctx);

            [DllImport(Libraries.Ssl)]
            public static extern IntPtr SSL_CTX_new(SslMethod method);

            [DllImport(Libraries.Ssl)]
            public static extern IntPtr SSL_CTX_set_client_cert_cb(SslMethod method);
        }
    }
}