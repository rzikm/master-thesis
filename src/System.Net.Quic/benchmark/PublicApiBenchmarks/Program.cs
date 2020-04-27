using BenchmarkDotNet.Running;

namespace PublicApiBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SslStreamComparisonBenchmarks>();
            // var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}