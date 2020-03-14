using System;

namespace OpenSSLSandbox
{
    public struct SslMethod
    {
        private readonly IntPtr handle;

        public SslMethod(IntPtr handle)
        {
            this.handle = handle;
        }

        public static implicit operator SslMethod(IntPtr ptr)
        {
            return new SslMethod(ptr);
        }

        public override string ToString()
        {
            return handle.ToString("x");
        }
    }
}