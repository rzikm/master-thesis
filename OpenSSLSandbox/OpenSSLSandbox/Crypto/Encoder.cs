using System;
using System.ComponentModel;

namespace OpenSSLSandbox.Crypto
{
    public class Encoder
    {
        private static int GetVarIntLogLength(ulong value)
        {
            if (value <= 63) return 0;
            if (value <= 16_383) return 1;
            if (value <= 1_073_741_823) return 2;
            if (value <= 4_611_686_018_427_387_903) return 3;

            throw new ArgumentOutOfRangeException(nameof(value));
        }
        
        private static int ReadVarIntLength(byte firstByte)
        {
            switch (firstByte >> 6)
            {
                case 00: return 1;
                case 01: return 2;
                case 10: return 4;
                case 11: return 8;
                default: // Unreachable
                    throw new InvalidOperationException();
            }
        }
        
        /// <summary>
        /// Encodes a variable length integer, returns number of bytes written.
        /// </summary>
        /// <param name="value">Integer value to be encoded.</param>
        /// <param name="memory">Target memory to be encoded into.</param>
        /// <returns></returns>
        public static int EncodeVarInt(ulong value, Span<byte> memory)
        {
            var log = GetVarIntLogLength(value);
            var bytes = 1 << log;
            
            if (memory.Length < bytes) throw new ArgumentException("Buffer too short");

            // prefix with log length
            value |= (ulong) log << (bytes * 8 - 2);

            for (int i = 0; i < bytes; i++)
            {
                memory[bytes - i - 1] = (byte) value;
                value >>= 8;
            }

            return bytes;
        }

        /// <summary>
        /// Decodes a variable length encoding from the given memory, returns number of bytes read.
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int DecodeVarInt(ReadOnlySpan<byte> memory, out ulong value)
        {
            // first two bits give logarithm of size
            var logBytes = memory[0] >> 6;
            var bytes = 1 << logBytes;
            
            if (memory.Length < bytes) throw new ArgumentException("Buffer too short");

            ulong v = (ulong) (memory[0] & 0b0011_1111);

            for (int i = 1; i < bytes; i++)
            {
                v = (v << 8) | memory[i];
            }

            value = v;
            return bytes;
        }
    }
}