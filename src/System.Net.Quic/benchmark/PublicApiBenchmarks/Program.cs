using System;
using BenchmarkDotNet.Running;

namespace PublicApiBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Environment.SetEnvironmentVariable("USE_MSQUIC", "1");
            // var summary = BenchmarkRunner.Run<ConnectionEstablishmentComparisonBenchmarks>();
            // var summary = BenchmarkRunner.Run<ConnectionCloseComparisonBenchmarks>();
            var summary = BenchmarkRunner.Run<StreamPerformanceComparisonBenchmarks>();
            // var summary = BenchmarkRunner.Run<WorkbenchBenchmark>();
            // var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}