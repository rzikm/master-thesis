using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
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
            if (splits.Length != 2) throw new ArgumentException("Value must contain exactly one :");

            IPAddress a;
            if (splits[0] == "*")
                a = IPAddress.Any;
            else
                a = Dns.GetHostAddresses(splits[0]).First();

            int port = ushort.Parse(splits[1]);
            if (port == 0) throw new ArgumentException("Port must not be 0");

            return new IPEndPoint(a, port);
        }
    }
}