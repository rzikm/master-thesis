using OpenSSLSandbox.Crypto;
using Xunit;

namespace Tests
{
    public class EncoderTest
    {
        [Fact]
        public void TestVarintByte()
        {
            const byte value = 37;
            
            var actual = new byte[1];
            var expected = new byte[] {0x25};
            
            Encoder.EncodeVarIntByte(value, actual);
            
            AssertEncodingCorrect(expected, actual, value, 1);
        }

        private static void AssertEncodingCorrect(byte[] expected, byte[] actual, long value, int bytes)
        {
            Assert.Equal(expected, actual);
            Assert.Equal(bytes, Encoder.DecodeVarInt(actual, out var decoded));
            Assert.Equal(value, decoded);
        }

        [Fact]
        public void TestVarintTwoBytes()
        {
            const short value = 15293;
            
            var actual = new byte[2];
            var expected = new byte[] {0x7b, 0xbd};
            
            Encoder.EncodeVarIntTwoByte(value, actual);
            
            AssertEncodingCorrect(expected, actual, value, 2);
        }
        
        [Fact]
        public void TestVarintFourBytes()
        {
            const int value = 494878333;
            
            var actual = new byte[4];
            var expected = new byte[] {0x9d, 0x7f, 0x3e, 0x7d};
            
            Encoder.EncodeVarIntFourByte(value, actual);
            
            AssertEncodingCorrect(expected, actual, value, 4);
        }
        
        [Fact]
        public void TestVarintEightBytes()
        {
            const long value = 151288809941952652;
            
            var actual = new byte[8];
            var expected = new byte[] {0xc2, 0x19, 0x7c, 0x5e, 0xff, 0x14, 0xe8, 0x8c};
            
            Encoder.EncodeVarIntEightByte(value, actual);
            
            AssertEncodingCorrect(expected, actual, value, 8);
        }
    }
}