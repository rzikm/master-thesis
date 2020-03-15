namespace OpenSSLSandbox.Interop
{
    internal static class Libraries
    {
        #if LINUX
        
        // using local installation for now
        private const string LibPrefix = @"/usr/local/lib/";
        public const string Ssl = LibPrefix + "libssl.so";
        public const string Crypto = LibPrefix + "libcrypto.so";
        
        #elif WINDOWS
        
        // TODO: sort out 32-64bit madness
        private const string LibPrefix = @"Lib\win-x86\";
        public const string Ssl = LibPrefix + "libssl-1_1.dll";
        public const string Crypto = LibPrefix + "libcrypto-1_1.dll";
        
        #endif
    }
}