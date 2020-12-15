using System;
using System.Net;
using System.Threading;
using CommandLine;

namespace ThroughputTests
{
    internal class ClientCommonOptions
    {
        [Option('c', "connections", Default = 256, HelpText = "Number of connections made.")]
        public int Connections { get; set; }

        [Option('s', "streams", Default = 1, HelpText = "Number of streams each client opens. Valid only for QUIC.")]
        public int Streams { get; set; }
        
        [Option('t', "tcp", Default = false, HelpText = "Use TCP+TLS (SslStream) instead of QUIC.")]
        public bool Tcp { get; set; }

        [Option('m', "message-size", Default = 256, HelpText = "Message size.")]
        public int MessageSize { get; set; }

        [Option('i', "reporting-interval", Default = 3, HelpText = "Reporting interval in seconds.")] // Seconds
        public double ReportingInterval { get; set; }
        
        [Option('d', "test-duration", Default = 0, HelpText = "Duration of the test in seconds, value 0 will run the test indefinitely.")] // Seconds
        public double DurationTime { get; set; }
        
        [Option('w', "warmup-time", Default = 5, HelpText = "Time before starting measurements")] // Seconds
        public double WarmupTime { get; set; }
        
        [Option('n', "no-wait", Default = false, HelpText = "Don't wait for the previous request to finish before sending another one")]
        public bool NoWait { get; set; }
        
        [Option("csv-output", Default = false, HelpText = "Prints output formatted as CSV with , separators")]
        public bool CsvOutput { get; set; }
    }

    [Verb("client", HelpText = "Run client connecting to the specified address")]
    internal class ClientOptions : ClientCommonOptions
    {
        [Option('e', "endpoint", Required = true, HelpText = "Endpoint to connect to.")]
        public string EndPointString
        {
            set => EndPoint = Helpers.ResolveEndPoint(value);
        }

        public IPEndPoint EndPoint { get; private set; }
    }

    [Verb("server", HelpText = "Run server listening on the specified address")]
    internal class ServerOptions
    {
        [Option('e', "endpoint", Default = "*:5000", HelpText = "Endpoint to listen on")]
        public string EndPointString
        {
            set => EndPoint = Helpers.ResolveEndPoint(value);
        }

        public IPEndPoint EndPoint { get; private set; }

        [Option("max-threads", Default = 0)]
        public int MaxThreads
        {
            set
            {
                if (value == 0) return;

                ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var _);
                if (!ThreadPool.SetMaxThreads(maxWorkerThreads, value))
                    throw new InvalidOperationException("ThreadPool.SetMaxThreads failed");
            }
        }

        [Option("certificate-file", HelpText = "Path to the certificate public key file", Required = true)]
        public string CertificateFile { get; set; }

        [Option("key-file", HelpText = "Path to the certificate private key file", Required = true)]
        public string PrivateKeyFile { get; set; }
    }

    [Verb("inproc", HelpText = "Run client and server locally over loopback")]
    internal class InProcOptions : ClientCommonOptions
    {

        [Option("certificate-file", HelpText = "Path to the certificate public key file", Required = true)]
        public string CertificateFile { get; set; }

        [Option("key-file", HelpText = "Path to the certificate private key file", Required = true)]
        public string PrivateKeyFile { get; set; }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            void CancelHandler(object sender, ConsoleCancelEventArgs args)
            {
                args.Cancel = true;
                cts.Cancel();
                Console.CancelKeyPress -= CancelHandler!;
            }

            Console.CancelKeyPress += CancelHandler;
            
            return new Parser(settings => { settings.HelpWriter = Console.Out; })
                .ParseArguments<ClientOptions, ServerOptions, InProcOptions>(args)
                .MapResult(
                    (ClientOptions opts) => ClientRunner.Run(opts, cts),
                    (ServerOptions opts) => ServerRunner.Run(opts, cts.Token),
                    (InProcOptions opts) => InProcRunner.Run(opts, cts),
                    _ => 1);
        }
    }
}