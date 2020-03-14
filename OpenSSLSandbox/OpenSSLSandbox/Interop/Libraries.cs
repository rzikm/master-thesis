namespace OpenSSLSandbox.Interop
{
    internal static class Libraries
    {
        // public const string Ssl = @"C:\Program Files (x86)\OpenSSL\bin\libssl-1_1.dll";
        // public const string Crypto = @"C:\Program Files (x86)\OpenSSL\bin\libcrypto-1_1.dll";

        // TODO: sort out 32-64bit Linux/Windows madness
        private const string LibPrefix = @"Lib\win-x86\";
        public const string Ssl = LibPrefix + "libssl-1_1.dll";
        public const string Crypto = LibPrefix + "libcrypto-1_1.dll";
    }
}