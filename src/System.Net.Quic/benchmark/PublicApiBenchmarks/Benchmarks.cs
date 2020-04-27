using System;
using System.Net.Quic.Public;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace PublicApiBenchmarks
{
    [MemoryDiagnoser]
    [InProcess]
    [Config(typeof(Config))]
    public class Benchmarks
    {
        class Config : ManualConfig
        {
            public Config()
            {
                Options |= ConfigOptions.DisableOptimizationsValidator;
            }
        }
        
        private QuicListener _listener;
        private QuicConnection _client;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _listener = QuicFactory.CreateListener();
            _listener.Start();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _client = QuicFactory.CreateClient(_listener.ListenEndPoint);
        }
        
        [Benchmark]
        public async Task EstablishConnection()
        {
            await _client.ConnectAsync();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _client.Dispose();
            _listener.AcceptConnectionAsync().Result.Dispose();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _listener.Close();
            _listener.Dispose();
        }
    }
}