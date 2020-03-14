using System;
using System.Globalization;
using OpenSSLSandbox.Crypto;
using Xunit;

namespace Tests
{
    public class CryptoTests
    {
        private static string ToHexString(byte[] data)
        {
            return BitConverter.ToString(data).ToLower().Replace("-", "");
        }

        private static byte[] FromHexString(string hex)
        {
            var buf = new byte[hex.Length / 2];
            for (var i = 0; i < buf.Length; i++) buf[i] = byte.Parse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber);

            return buf;
        }

        private static readonly string dcid = "8394c8f03e515708";

        private static readonly string initialSecret =
            "524e374c6da8cf8b496f4bcb69678350" +
            "7aafee6198b202b4bc823ebf7514a423";

        [Theory]
        [InlineData("client in", 32, "00200f746c73313320636c69656e7420696e00")]
        [InlineData("server in", 32, "00200f746c7331332073657276657220696e00")]
        [InlineData("quic key", 16, "00100e746c7331332071756963206b657900")]
        [InlineData("quic iv", 12, "000c0d746c733133207175696320697600")]
        [InlineData("quic hp", 16, "00100d746c733133207175696320687000")]
        public void TestLabelGeneration(string label, ushort len, string expectedHex)
        {
            var actual = KeyDerivation.CreateHkdfLabel(label, len);
            var actualHex = ToHexString(actual);

            Assert.Equal(expectedHex, actualHex);
        }

        [Fact]
        public void TestInitialSecret()
        {
            var actual = KeyDerivation.DeriveInitialSecret(FromHexString(dcid));
            var actualHex = ToHexString(actual);

            Assert.Equal(initialSecret, actualHex);
        }

        [Fact]
        public void TestInitialClientKeyingMaterial()
        {
            var initial = KeyDerivation.DeriveClientInitialSecret(FromHexString(initialSecret));

            var clientInitial =
                "fda3953aecc040e48b34e27ef87de3a6" +
                "098ecf0e38b7e032c5c57bcbd5975b84";

            Assert.Equal(clientInitial, ToHexString(initial));

            var key = KeyDerivation.DeriveKey(initial);
            Assert.Equal("af7fd7efebd21878ff66811248983694", ToHexString(key));

            var iv = KeyDerivation.DeriveIv(initial);
            Assert.Equal("8681359410a70bb9c92f0420", ToHexString(iv));

            var hp = KeyDerivation.DeriveHp(initial);
            Assert.Equal("a980b8b4fb7d9fbc13e814c23164253d", ToHexString(hp));
        }

        [Fact]
        public void TestInitialServerKeyingMaterial()
        {
            var initial = KeyDerivation.DeriveServerInitialSecret(FromHexString(initialSecret));

            var serverInitial =
                "554366b81912ff90be41f17e80222130" +
                "90ab17d8149179bcadf222f29ff2ddd5";

            Assert.Equal(serverInitial, ToHexString(initial));

            var key = KeyDerivation.DeriveKey(initial);
            Assert.Equal("5d51da9ee897a21b2659ccc7e5bfa577", ToHexString(key));

            var iv = KeyDerivation.DeriveIv(initial);
            Assert.Equal("5e5ae651fd1e8495af13508b", ToHexString(iv));

            var hp = KeyDerivation.DeriveHp(initial);
            Assert.Equal("a8ed82e6664f865aedf6106943f95fb8", ToHexString(hp));
        }
    }
}