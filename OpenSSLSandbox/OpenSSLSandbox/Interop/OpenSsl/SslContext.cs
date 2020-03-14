using System;

namespace OpenSSLSandbox
{
    public struct SslContext
    {
        private readonly IntPtr handle;

        public SslContext(IntPtr handle)
        {
            this.handle = handle;
        }

        public static implicit operator SslContext(IntPtr ptr)
        {
            return new SslContext(ptr);
        }

        public override string ToString()
        {
            return handle.ToString("x");
        }
    }
}