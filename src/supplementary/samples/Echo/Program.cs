using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace Echo
{
    class Program
    {
        [Verb("client", HelpText = "Run client connecting to the specified address")]
        internal class ClientOptions
        {
            [Option('e', "endpoint", Required = true, HelpText = "Endpoint to connect to.")]
            public string EndPointString
            {
                set => EndPoint = Helpers.ResolveEndPoint(value);
            }

            public IPEndPoint EndPoint { get; private set; }
        }

        [Verb("server", HelpText = "Run server listening on the specified address")]
        class ServerOptions
        {
            [Option('e', "endpoint", Default = "*:5000", HelpText = "Endpoint to listen on")]
            public string EndPointString
            {
                set => EndPoint = Helpers.ResolveEndPoint(value);
            }

            public IPEndPoint EndPoint { get; private set; }

            [Option('c', "certificate-file", HelpText = "Path to the certificate public key file", Required = true)]
            public string CertificateFile { get; set; }

            [Option('k', "key-file", HelpText = "Path to the certificate private key file", Required = true)]
            public string PrivateKeyFile { get; set; }
        }

        static async Task<int> Main(string[] args)
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            void CancelHandler(object sender, ConsoleCancelEventArgs args)
            {
                args.Cancel = true;
                cts.Cancel();
                Console.CancelKeyPress -= CancelHandler!;
            }

            Console.CancelKeyPress += CancelHandler;

            try
            {
                return await new Parser(settings => { settings.HelpWriter = Console.Out; })
                    .ParseArguments<ClientOptions, ServerOptions>(args)
                    .MapResult(
                        (ClientOptions opts) => Echo.RunClient(opts.EndPoint, cts.Token),
                        (ServerOptions opts) => Echo.RunServer(opts.EndPoint, opts.CertificateFile, opts.PrivateKeyFile, cts.Token),
                        _ => Task.FromResult(1));
            }
            catch (OperationCanceledException)
            {
                return 1;
            }
        }
    }
}