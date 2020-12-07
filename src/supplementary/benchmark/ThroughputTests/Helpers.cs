using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    internal static class Helpers
    {
        internal static readonly SslApplicationProtocol AlpnProtocol = new SslApplicationProtocol("thpttst");

        [Conditional("DEBUG")]
        internal static void Trace(string message)
        {
            Console.WriteLine(message);
        }

        internal static void InterruptibleWait(int milliseconds, CancellationToken cancellationToken)
        {
            try
            {
                Task.Delay(milliseconds).Wait(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }

        internal static byte[] CreateMessageBuffer(int size)
        {
            // Create zero-terminated message of the specified length
            var buffer = new byte[size];
            for (var i = 0; i < size - 1; i++) buffer[i] = 0xFF;

            buffer[size - 1] = 0;
            return buffer;
        }

        internal static Task Dispatch(Func<Task> task)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unhandled exception: {e}");
                }
            });
        }

        internal static IPEndPoint ResolveEndPoint(string input)
        {
            if (IPEndPoint.TryParse(input, out var value)) return value;

            var splits = input.Split(':');
            if (splits.Length != 2) throw new ArgumentException("Value must contain exactly one ':' character");

            IPAddress a;
            if (splits[0] == "*")
                a = IPAddress.Any;
            else
                a = Dns.GetHostAddresses(splits[0]).First();

            int port = ushort.Parse(splits[1]);
            if (port == 0) throw new ArgumentException("Port must not be 0");

            return new IPEndPoint(a, port);
        }

        internal static X509Certificate2 LoadCertificate(string certificatePath, string privateKeyPath)
        {
            static byte[] GetBytesFromPem(string pemString, bool isCert)
            {
                string header; string footer;
                
                if (isCert)
                {
                    header = "-----BEGIN CERTIFICATE-----";
                    footer = "-----END CERTIFICATE-----";
                }
                else
                {
                    header = "-----BEGIN PRIVATE KEY-----";
                    footer = "-----END PRIVATE KEY-----";
                }
            
                int start = pemString.IndexOf(header, StringComparison.Ordinal) + header.Length;
                int end = pemString.IndexOf(footer, start, StringComparison.Ordinal) - start;
                return Convert.FromBase64String(pemString.Substring(start, end));
            }
            
            byte[] certBuffer = GetBytesFromPem(File.ReadAllText(certificatePath), true);
            byte[] keyBuffer  = GetBytesFromPem(File.ReadAllText(privateKeyPath), false);
            
            X509Certificate2 certificate = new X509Certificate2(certBuffer, "");
            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(keyBuffer, out _);
            certificate = certificate.CopyWithPrivateKey(rsa);
            
            return new X509Certificate2(certificate.Export(X509ContentType.Pkcs12));
        }
    }
}