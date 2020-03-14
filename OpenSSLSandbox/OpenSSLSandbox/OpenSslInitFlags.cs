namespace OpenSSLSandbox
{
    public enum OpenSslInitFlags : long
    {
        LoadSslStrings = 0x00200000L,
        LoadCryptoStrings = 0x00000002L
    }
}