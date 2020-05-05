using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace PublicApiBenchmarks
{
    public class QuicDiagnoser : EventListener, IDiagnoser
    {
        private static readonly string DiagnoserId = nameof(QuicDiagnoser);

        public static readonly QuicDiagnoser Default = new QuicDiagnoser();

        private EventSource _quicEventSource;

        // event ids, need to be synchronized with ones used in NetEventSource.Quic
        private const int PacketSentId = 17;
        private const int PacketLostId = PacketSentId + 1;

        private struct EventData
        {
            public int PacketsSent;
            public int BytesSent;
            public int PacketsLost;
            public int BytesLost;
        }

        private EventData _data;

        public RunMode GetRunMode(BenchmarkCase benchmarkCase)
        {
            return benchmarkCase.Descriptor.WorkloadMethodDisplayInfo.Contains("Quic")
                ? RunMode.ExtraRun
                : RunMode.None;
        }

        public void Handle(HostSignal signal, DiagnoserActionParameters parameters)
        {
            if (signal == HostSignal.BeforeActualRun)
            {
                // reset counters
                _data = new EventData();
                EnableEvents(_quicEventSource, EventLevel.Verbose);
            }

            if (signal == HostSignal.AfterActualRun)
            {
                DisableEvents(_quicEventSource);
            }
        }

        public IEnumerable<Metric> ProcessResults(DiagnoserResults results)
        {
            var ops = (double) results.TotalOperations;
            yield return new Metric(PacketCountMetric.PacketsSent, _data.PacketsSent / ops);
            yield return new Metric(DataSentMetric.BytesSent, _data.BytesSent / ops);
            yield return new Metric(PacketCountMetric.PacketsLost, _data.PacketsLost / ops);
            yield return new Metric(DataSentMetric.BytesLost, _data.BytesLost / ops);
        }

        public void DisplayResults(ILogger logger)
        {
        }

        public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters) =>
            Array.Empty<ValidationError>();

        public IEnumerable<string> Ids => new[] {DiagnoserId};
        public IEnumerable<IExporter> Exporters => Array.Empty<IExporter>();
        public IEnumerable<IAnalyser> Analysers => Array.Empty<IAnalyser>();


        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);

            if (eventSource.Name == "Microsoft-System-Net-Quic")
            {
                _quicEventSource = eventSource;
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource == _quicEventSource)
            {
                switch (eventData.EventId)
                {
                    case PacketSentId:
                        Interlocked.Increment(ref _data.PacketsSent);
                        Interlocked.Add(ref _data.BytesSent, (int) eventData.Payload[0]);
                        break;
                    case PacketLostId:
                        Interlocked.Increment(ref _data.PacketsLost);
                        Interlocked.Add(ref _data.BytesLost, (int) eventData.Payload[0]);
                        break;
                }
            }
        }

        private class PacketCountMetric : IMetricDescriptor
        {
            public static readonly IMetricDescriptor PacketsSent = new PacketCountMetric("Packets sent");
            public static readonly IMetricDescriptor PacketsLost = new PacketCountMetric("Packets lost");

            public PacketCountMetric(string displayName)
            {
                DisplayName = displayName;
            }

            public string Id => $"{nameof(QuicDiagnoser)}.{DisplayName}";
            public string DisplayName { get; }
            public string Legend => "";
            public string NumberFormat => "#0.0000";
            public UnitType UnitType => UnitType.Dimensionless;
            public string Unit => "Count";
            public bool TheGreaterTheBetter => false;
        }

        private class DataSentMetric : IMetricDescriptor
        {
            public static readonly IMetricDescriptor BytesSent = new DataSentMetric("Bytes sent");
            public static readonly IMetricDescriptor BytesLost = new DataSentMetric("Bytes lost");

            public DataSentMetric(string displayName)
            {
                DisplayName = displayName;
            }

            public string Id => $"{nameof(QuicDiagnoser)}.{DisplayName}";
            public string DisplayName { get; }
            public string Legend => "";
            public string NumberFormat => "N0";
            public UnitType UnitType => UnitType.Size;
            public string Unit => SizeUnit.B.Name;
            public bool TheGreaterTheBetter => false;
        }
    }
}