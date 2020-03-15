using System;
using System.Linq;
using System.Security.Cryptography;

namespace OpenSSLSandbox.Crypto
{
    public enum Algorithm
    {
        AEAD_AES_128_GCM,
    }
    
    public class Encryption
    {
        public static byte[] GetInitialNonce(byte[] iv, ulong packetNumber)
        {
            var nonce = (byte[]) iv.Clone();

            for (int i = 0; i < 8; i++)
            {
                nonce[nonce.Length - 1 - i] ^= (byte) (packetNumber >> (i * 8));
            }

            return nonce;
        }
        
        public static byte[] GetHeaderProtectionMask(Algorithm alg, byte[] key, byte[] sample)
        {
            var aead = new AesManaged()
            {
                KeySize = 128,
                Mode = CipherMode.ECB,
                Key = key,
            };

            return aead.CreateEncryptor().TransformFinalBlock(sample, 0, sample.Length).Take(5).ToArray();
        }
    }
}