namespace OpenSSLSandbox
{
    public interface IQuicCallback
    {
        int SetEncryptionSecrets(SslEncryptionLevel level, byte[] readSecret, byte[] writeSecret);
        int AddHandshakeData(SslEncryptionLevel level, byte[] data);
        int Flush();
        int SendAlert(SslEncryptionLevel level, TlsAlert alert);
    }
}