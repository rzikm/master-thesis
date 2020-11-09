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
            Console.WriteLine(
                $"Test running with {options.Connections} connections, {options.Streams} per connection and {options.MessageSize}B messages");

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

            List<Column> columns = new List<Column>
            {
                new Column("Timestamp", 16, () => $"{elapsed}"),
                new Column("Current RPS", 16, () => $"{currentRps:####.0}"),
                new Column("Average RPS", 16, () => $"{averageRps:####.0}"),
                new Column("Drift", 10, () => previousAverageRps == 0.0? "" : $"{drift:0.00}%"),
                new Column("Throughput", 14, () => $"{dataThroughput:####.0} MiB/s"),
            };

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

                PrintColumns(columns);

                previousElapsed = elapsed;
                previousCount = totalCount;
                previousAverageRps = averageRps;
            }
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