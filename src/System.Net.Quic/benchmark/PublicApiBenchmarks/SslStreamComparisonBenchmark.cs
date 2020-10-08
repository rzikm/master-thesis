using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;

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
        
        private Channel<int> _connectionSignalChannel;
        private Task _serverTask;
        private const string CertFilePath = "Certs/cert.crt";
        private const string CertPrivateKeyPath = "Certs/cert.key";
        private const string CertPfx = "Certs/cert-combined.pfx";

        protected class Config : ManualConfig
        {
            public Config()
            {
                // for debug purposes
                Options |= ConfigOptions.DisableOptimizationsValidator;
                
                AddDiagnoser(MemoryDiagnoser.Default);
                // AddDiagnoser(ThreadingDiagnoser.Default);
                
                // AddDiagnoser(QuicDiagnoser.Default);
                AddJob(Job.InProcess.WithIterationCount(30));
                // AddJob(Job.InProcess);
                
                // AddJob(Job.Default);
            }
        }

        protected void DoGlobalSetupShared()
        {
            _connectionSignalChannel = Channel.CreateUnbounded<int>();
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

        private async Task QuicStreamServer()
        {
            await foreach (var _ in _connectionSignalChannel.Reader.ReadAllAsync())
            {
                var connection = await QuicListener.AcceptConnectionAsync();
                await QuicStreamServer(connection);
                connection.Dispose();
            }
        }

        protected abstract Task QuicStreamServer(QuicConnection connection);
        
        protected virtual void GlobalSetupQuicStream() {}

        [GlobalSetup(Target = nameof(SslStream))]
        public void DoGlobalSetupSslStream()
        {
            DoGlobalSetupShared();
            TcpListener = new TcpListener(IPAddress.IPv6Loopback, 0);
            TcpListener.Start();
            _serverTask = Task.Run(SslStreamServer);
            GlobalSetupSslStream();
        }
        
        protected virtual void GlobalSetupSslStream() {}

        private async Task SslStreamServer()
        {
            await foreach (var _ in _connectionSignalChannel.Reader.ReadAllAsync())
            {
                using var client = await TcpListener.AcceptTcpClientAsync();
                await using var stream = new SslStream(client.GetStream(), false);
                var cert = new X509Certificate2(CertPfx);
                await stream.AuthenticateAsServerAsync(cert);
                await SslStreamServer(stream);
            }
        }

        protected abstract Task SslStreamServer(SslStream stream);
        
        [IterationSetup(Target = nameof(QuicStream))]
        public void DoIterationSetupQuicStream()
        {
            _connectionSignalChannel.Writer.TryWrite(0);
            IterationSetupQuicStream();
        }
        
        protected virtual void IterationSetupQuicStream() { }

        [IterationSetup(Target = nameof(SslStream))]
        public void DoIterationSetupSslStream()
        {
            _connectionSignalChannel.Writer.TryWrite(0);
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
            _connectionSignalChannel.Writer.Complete();
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
        
        protected static SslStream CreateSslStream(Stream innerStream)
        {
            return new SslStream(innerStream, false, (sender, certificate, chain, errors) => true);
        }
        
        [GlobalSetup(Target = "MsQuic")]
        public void GlobalSetupMsQuic()
        {
            Environment.SetEnvironmentVariable("USE_MSQUIC", "1");
            DoGlobalSetupQuicStream();
        }
        
        [IterationSetup(Target = "MsQuic")]
        public void IterationSetupMsQuic()
        {
            DoIterationSetupQuicStream();
        }

        [IterationCleanup(Target = "MsQuic")]
        public void IterationCleanupMsQuic()
        {
            DoIterationCleanupQuicStream();
        }

        [GlobalCleanup(Target = "MsQuic")]
        public void GlobalCleanupMsQuic()
        {
            Environment.SetEnvironmentVariable("USE_MSQUIC", null);
            DoGlobalCleanupQuicStream();
        }
    }
}