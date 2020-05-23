using System;
using BenchmarkDotNet.Running;

namespace PublicApiBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Environment.SetEnvironmentVariable("USE_MSQUIC", "1");
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}