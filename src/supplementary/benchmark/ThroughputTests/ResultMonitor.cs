using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    struct Column
    {
        public Column(string header, int width, Func<string> valueFactory)
        {
            Header = header;
            Width = width;
            ValueFactory = valueFactory;
        }

        public string Header { get; set; }
        public int Width { get; set; }
        public Func<string> ValueFactory { get; set; }
    }

    internal static class ResultMonitor
    {
        internal static void MonitorResults(Client[] clients, ClientCommonOptions options,
            CancellationToken cancellationToken)
        {
            if (options.Tcp)
            {
                Console.WriteLine(
                    $"Test running SSL with {options.Connections} connections and {options.MessageSize}B messages");
            }
            else
            {
                Console.WriteLine(
                    $"Test running QUIC with {options.Connections} connections, {options.Streams} streams per connection and {options.MessageSize}B messages");
            }

            int GetCurrentRequestCount()
            {
                return clients.Sum(c => c.GetRequestCount());
            }

            long startCount = 0;
            if (options.WarmupTime > 0)
            {
                Helpers.InterruptibleWait((int) (options.WarmupTime * 1000), cancellationToken);
                startCount = GetCurrentRequestCount();
            }

            long previousCount = startCount;
            Stopwatch sw = Stopwatch.StartNew();
            TimeSpan previousElapsed = TimeSpan.Zero;
            TimeSpan elapsed = TimeSpan.Zero;

            double currentRps = 0;
            double averageRps = 0;
            double dataThroughput = 0;
            double previousAverageRps = 0;
            double drift = 0;
            double avgLatency = 0;
            double p95Latency = 0;
            double p99Latency = 0;

            List<TimeSpan>[][] latencies = Enumerable.Range(0, options.Connections).Select(_ =>
                    Enumerable.Range(0, Math.Max(options.Streams, 1)).Select(_ => new List<TimeSpan>()).ToArray())
                .ToArray();

            List<Column> columns = new List<Column>
            {
                new Column("Timestamp", 16, () => $"{elapsed}"),
                new Column("Current RPS", 16, () => $"{currentRps:####.0}"),
                new Column("Average RPS", 16, () => $"{averageRps:####.0}"),
                new Column("Drift (%)", 11, () => previousAverageRps == 0.0 ? "" : $"{drift:0.00}"),
                new Column("Throughput (MiB/s)", 20, () => $"{dataThroughput:####.0}"),
            };

            if (!options.NoWait)
            {
                // latency measurements make sense only when we are waiting for the server's reply
                columns.Add(new Column("Latency-avg (ms)", 20, () => $"{avgLatency:####.000}"));
                columns.Add(new Column("Latency-p95 (ms)", 20, () => $"{p95Latency:####.000}"));
                columns.Add(new Column("Latency-p99 (ms)", 20, () => $"{p99Latency:####.000}"));
            }

            PrintHeader(columns);

            while (!cancellationToken.IsCancellationRequested)
            {
                Helpers.InterruptibleWait((int) (options.ReportingInterval * 1000), cancellationToken);

                elapsed = sw.Elapsed;
                long totalCount = GetCurrentRequestCount();

                currentRps = (totalCount - previousCount) / (elapsed - previousElapsed).TotalSeconds;
                averageRps = (totalCount - startCount) / elapsed.TotalSeconds;
                dataThroughput = currentRps * options.MessageSize / (1024 * 1024);
                drift = (averageRps - previousAverageRps) / previousAverageRps * 100;

                if (!options.NoWait)
                {
                    (avgLatency, p95Latency, p99Latency) = CalculateLatencies(clients, latencies);
                }

                PrintColumns(columns);

                previousElapsed = elapsed;
                previousCount = totalCount;
                previousAverageRps = averageRps;
            }
        }
        
        static List<double> gatheredLatencies = new List<double>();

        private static (double avg, double p95, double p99) CalculateLatencies(Client[] clients, List<TimeSpan>[][] latencies)
        {
            // swap latency measurements lists
            for (var i = 0; i < clients.Length; i++)
            {
                var c = clients[i];
                latencies[i] = c.ExchangeMeasurements(latencies[i]);
            }
            
            // gather them into one list
            gatheredLatencies.Clear();
            foreach (var c in latencies) {
                foreach (var list in c)
                {
                    gatheredLatencies.AddRange(list.Select(l => l.TotalMilliseconds));
                }
            }
            
            int count = gatheredLatencies.Count;
            double total = gatheredLatencies.Sum();

            gatheredLatencies.Sort();
            double CalculatePercentile(float percentile)
            {
                // TODO: interpolate between 2 elements
                return gatheredLatencies[(int) MathF.Floor(gatheredLatencies.Count * percentile)];
            }
            
            var avg = total / count;
            var p95 = gatheredLatencies.Count > 0 ? CalculatePercentile(0.95f) : double.NaN;
            var p99 = gatheredLatencies.Count > 0 ? CalculatePercentile(0.99f) : double.NaN;

            // clean up the lists for future use
            foreach (var c in latencies)
            {
                foreach (var list in c)
                {
                    list.Clear();
                }
            }

            return (avg, p95, p99);
        }

        private static void PrintColumns(List<Column> columns)
        {
            foreach (var c in columns)
            {
                Console.Write(c.ValueFactory().PadLeft(c.Width));
            }

            Console.WriteLine();
        }

        private static void PrintHeader(List<Column> columns)
        {
            foreach (var c in columns)
            {
                Console.Write(c.Header.PadLeft(c.Width));
            }

            Console.WriteLine();
            Console.WriteLine("".PadLeft(columns.Sum(c => c.Width), '-'));
        }
    }
}