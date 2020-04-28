using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace PublicApiBenchmarks
{
    [Config(typeof(Config))]
    public abstract class SslStreamComparisonBenchmark
    {
        protected QuicListener QuicListener;
        protected QuicConnection QuicClient;
        protected TcpListener TcpListener;
        protected TcpClient TcpClient;
        protected SslStream ClientSslStream;
        protected Channel<int> ConnectionSignalChannel;
        
        private Task _serverTask;
        private const string CertFilePath = "Certs/cert.crt";
        private const string CertPrivateKeyPath = "Certs/cert.key";
        protected const string CertPfx = "Certs/cert-combined.pfx";

        private class Config : ManualConfig
        {
            public Config()
            {
                // for debug purposes
                Options |= ConfigOptions.DisableOptimizationsValidator;
                // Add(Job.InProcess);
                
                Add(MemoryDiagnoser.Default);
                Add(Job.Default);
            }
        }

        protected void DoGlobalSetupShared()
        {
            ConnectionSignalChannel = Channel.CreateUnbounded<int>();
            GlobalSetupShared();
        }
        
        protected virtual void GlobalSetupShared() {}

        
        [GlobalSetup(Target = nameof(QuicStream))]
        public void DoGlobalSetupQuicStream()
        {
            DoGlobalSetupShared();
            QuicListener = QuicFactory.CreateListener();
            QuicListener.Start();
            _serverTask = Task.Run(QuicStreamServer);
            GlobalSetupQuicStream();
        }
        
        protected abstract Task QuicStreamServer();
        
        protected virtual void GlobalSetupQuicStream() {}

        [GlobalSetup(Target = nameof(SslStream))]
        public void DoGlobalSetupSslStream()
        {
            DoGlobalSetupShared();
            TcpListener = new TcpListener(IPAddress.Any, 0);
            TcpListener.Start();
            _serverTask = Task.Run(SslStreamServer);
            GlobalSetupSslStream();
        }
        
        protected virtual void GlobalSetupSslStream() {}

        protected abstract Task SslStreamServer();

        [IterationSetup(Target = nameof(QuicStream))]
        public void DoIterationSetupQuicStream()
        {
            ConnectionSignalChannel.Writer.TryWrite(0);
            IterationSetupQuicStream();
        }
        
        protected virtual void IterationSetupQuicStream() { }

        [IterationSetup(Target = nameof(SslStream))]
        public void DoIterationSetupSslStream()
        {
            ConnectionSignalChannel.Writer.TryWrite(0);
            IterationSetupSslStream();
        }
        
        protected virtual void IterationSetupSslStream() {}
        
        [IterationCleanup(Target = nameof(QuicStream))]
        public void DoIterationCleanupQuicStream()
        {
            IterationCleanupQuicStream();
        }
        
        protected virtual void IterationCleanupQuicStream() {}
        
        [IterationCleanup(Target = nameof(SslStream))]
        public void DoIterationCleanupSslStream()
        {
            IterationCleanupSslStream();
        }
        
        protected virtual void IterationCleanupSslStream() {}

        protected void StopServer()
        {
            ConnectionSignalChannel.Writer.Complete();
            _serverTask.Wait();
        }
        
        [GlobalCleanup(Target = nameof(QuicStream))]
        public void DoGlobalCleanupQuicStream()
        {
            StopServer();
            QuicListener.Dispose();
            GlobalCleanupQuicStream();
        }

        protected virtual void GlobalCleanupQuicStream() { }
        
        [GlobalCleanup(Target = nameof(SslStream))]
        public void DoGlobalCleanupSslStream()
        {
            StopServer();
            TcpListener.Stop();
            GlobalCleanupSslStream();
        }

        protected virtual void GlobalCleanupSslStream() { }
        
    }
}