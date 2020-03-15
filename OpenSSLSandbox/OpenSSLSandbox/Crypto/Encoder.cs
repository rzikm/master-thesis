using System;
using System.ComponentModel;

namespace OpenSSLSandbox.Crypto
{
    public class Encoder
    {
        /// <summary>
        /// Encodes a variable length integer, returns number of bytes written.
        /// </summary>
        /// <param name="value">Integer value to be encoded.</param>
        /// <param name="memory">Target memory to be encoded into.</param>
        /// <returns></returns>
        public static int EncodeVarInt(long value, Span<byte> memory)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Only nonnegative values may be encoded");
            // if ()

            return 0;
        }

        /// <summary>
        /// Encodes given value using one byte of variable-length encoding, as defined in Section 16 of QUIC-TRANSPORT.
        /// Supported range is 0-63.
        /// </summary>
        /// <param name="value">Value to be encoded.</param>
        /// <param name="memory">Target memory to be written into.</param>
        public static void EncodeVarIntByte(byte value, Span<byte> memory)
        {
            if (value > 63) throw new ArgumentOutOfRangeException(nameof(value));
            if (memory.Length == 0) throw new ArgumentException(nameof(memory));

            memory[0] = value;
        }
        
        /// <summary>
        /// Encodes given value using two bytes of variable-length encoding, as defined in Section 16 of QUIC-TRANSPORT.
        /// Supported range is 0-16383.
        /// </summary>
        /// <param name="value">Value to be encoded.</param>
        /// <param name="memory">Target memory to be written into.</param>
        public static void EncodeVarIntTwoByte(short value, Span<byte> memory)
        {
            if ((uint) value > 16383) throw new ArgumentOutOfRangeException(nameof(value));
            if (memory.Length < 2) throw new ArgumentException(nameof(memory));

            value |= (short)(01 << 14);

            memory[0] = (byte) (value >> 8);
            memory[1] = (byte) (value);
        }
        
        /// <summary>
        /// Encodes given value using four bytes of variable-length encoding, as defined in Section 16 of QUIC-TRANSPORT.
        /// Supported range is 0-1073741823.
        /// </summary>
        /// <param name="value">Value to be encoded.</param>
        /// <param name="memory">Target memory to be written into.</param>
        public static void EncodeVarIntFourByte(int value, Span<byte> memory)
        {
            if ((uint) value > 1073741823) throw new ArgumentOutOfRangeException(nameof(value));
            if (memory.Length < 4) throw new ArgumentException(nameof(memory));

            value |= 10 << 30;

            memory[0] = (byte) (value >> 24);
            memory[1] = (byte) (value >> 16);
            memory[2] = (byte) (value >> 8);
            memory[3] = (byte) value;
        }
        
        /// <summary>
        /// Encodes given value using four bytes of variable-length encoding, as defined in Section 16 of QUIC-TRANSPORT.
        /// Supported range is 0-4611686018427387903.
        /// </summary>
        /// <param name="value">Value to be encoded.</param>
        /// <param name="memory">Target memory to be written into.</param>
        public static void EncodeVarIntEightByte(long value, Span<byte> memory)
        {
            if ((uint) value > 4611686018427387903) throw new ArgumentOutOfRangeException(nameof(value));
            if (memory.Length < 4) throw new ArgumentException(nameof(memory));

            value |= 11L << 62;

            memory[0] = (byte) (value >> 56);
            memory[1] = (byte) (value >> 48);
            memory[2] = (byte) (value >> 40);
            memory[3] = (byte) (value >> 32);
            memory[4] = (byte) (value >> 24);
            memory[5] = (byte) (value >> 16);
            memory[6] = (byte) (value >> 8);
            memory[7] = (byte) value;
        }

        /// <summary>
        /// Decodes a variable length encoding from the given memory, returns number of bytes read.
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int DecodeVarInt(ReadOnlySpan<byte> memory, out long value)
        {
            // first two bits give logarithm of size
            var logBytes = memory[0] >> 6;
            var bytes = 1 << logBytes;
            
            if (memory.Length < bytes) throw new ArgumentException("Buffer too short");
            
            long v = memory[0] & 0b0011_1111;

            for (int i = 1; i < bytes; i++)
            {
                v = (v << 8) | memory[i];
            }

            value = v;
            return bytes;
        }
    }
}