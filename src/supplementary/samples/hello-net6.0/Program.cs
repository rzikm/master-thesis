using System;
using System.Diagnostics;
using System.IO;
using System.Net.Quic;

namespace hello_net6._0
{
    class Program
    {
        static void Main(string[] args)
        {
            string assemblyPath = typeof(object).Assembly.Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            var info = FileVersionInfo.GetVersionInfo(assemblyPath);
            Console.WriteLine($"Hello World from .NET {info.ProductVersion}");
            Console.WriteLine($"Runtime location: {assemblyDir}");
            Console.WriteLine($"QUIC: {QuicImplementationProviders.Default}");
        }
    }
}
