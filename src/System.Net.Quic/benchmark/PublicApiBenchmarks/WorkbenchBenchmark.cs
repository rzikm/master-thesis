using System;
using System.IO;
using System.Net.Quic;
using System.Net.Security;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

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
}