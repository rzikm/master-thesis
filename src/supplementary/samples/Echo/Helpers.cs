using System;
using System.Linq;
using System.Net;

namespace Echo
{
    internal static class Helpers
    {
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