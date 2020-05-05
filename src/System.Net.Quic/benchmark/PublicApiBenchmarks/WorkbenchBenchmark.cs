using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace PublicApiBenchmarks
{
    [Config(typeof(Config))]
    public class WorkbenchBenchmark : SslStreamComparisonBenchmark
    {
        private new class Config : SslStreamComparisonBenchmark.Config
        {
            public Config()
            {
                // Add(QuicDiagnoser.Default);
            }
        }
        
        // [Params(64 * 1024, 1024 * 1024, 32 * 1024 * 1024)]
        // [Params(1024 * 1024, 32 * 1024 * 1024)]
        [Params(32 * 1024, 16 * 1024)]
        // [Params(32 * 1024)]
        // [Params(1024 * 1024)]
        public int DataLength { get; set; }

        public int SendBufferSize { get; set; } = 16 * 1024;

        public int RecvBufferSize { get; set; } = 16 * 1024;

        private byte[] _recvBuffer;
        private byte[] _sendBuffer;

        protected sealed override void GlobalSetupShared()
        {
            _sendBuffer = new byte[SendBufferSize];
            _recvBuffer = new byte[RecvBufferSize];
        }

        protected sealed override async Task QuicStreamServer(QuicConnection connection)
        {
            await using var stream = connection.OpenUnidirectionalStream();
            await SendData(stream);
            await stream.ShutdownWriteCompleted();
        }

        protected sealed override Task SslStreamServer(SslStream stream)
        {
            // unused
            throw new NotImplementedException();
        }

        private async Task SendData(Stream stream)
        {
            int written = 0;
            while (written < DataLength)
            {
                await stream.WriteAsync(_sendBuffer);
                written += _sendBuffer.Length;
            }

            await stream.FlushAsync();
        }

        private async Task RecvData(Stream stream)
        {
            int recv;
            int total = 0;
            do
            {
                recv = await stream.ReadAsync(_recvBuffer);
                total += recv;
            } while (recv > 0);

            if (total != DataLength)
            {
                throw new InvalidOperationException("Received data size does not match send size");
            }
        }

        protected sealed override void IterationSetupQuicStream()
        {
            QuicClient = QuicFactory.CreateClient(QuicListener.ListenEndPoint);
            QuicClient.ConnectAsync().AsTask().GetAwaiter().GetResult();
        }

        [Benchmark]
        public async Task QuicStream()
        {
            await using var stream = await QuicClient.AcceptStreamAsync();
            await RecvData(stream);
        }

        protected sealed override void IterationCleanupQuicStream()
        {
            QuicClient.Dispose();
        }
    }
    
    public class EventListenerColumn : IColumn
    {
        private readonly Func<int> _getter;
        private readonly Dictionary<BenchmarkCase, int> _cache = new Dictionary<BenchmarkCase, int>();

        public EventListenerColumn(string columnName, UnitType unitType, Func<int> getter)
        {
            ColumnName = columnName;
            UnitType = unitType;
            _getter = getter;
        }
        
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            if (!_cache.TryGetValue(benchmarkCase, out int value))
            {
                _cache[benchmarkCase] = value = _getter();
            }

            value /= benchmarkCase.Job.Run.IterationCount * benchmarkCase.Job.Run.InvocationCount;

            if (UnitType != UnitType.Size)
            {
                return value.ToString();
            }

            var unit = SizeUnit.GetBestSizeUnit(value);
            double actual = SizeUnit.Convert(value, SizeUnit.B, unit);

            return $"{actual:F} {unit.Name}";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) 
            => GetValue(summary, benchmarkCase);

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public bool IsAvailable(Summary summary) => true;

        public string Id => $"{nameof(EventListenerColumn)}.{ColumnName}";
        public string ColumnName { get; }
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Metric;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType { get; }
        public string Legend => "Number of packets lost";
    }
}