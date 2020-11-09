using System;
using System.Threading;

namespace ThroughputTests
{
    internal class ServerRunner
    {
        public static int Run(ServerOptions opts, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Running server on {opts.EndPoint}");

            ServerListener.Start(opts.EndPoint, opts.CertificateFile, opts.PrivateKeyFile, cancellationToken);
            Thread.Sleep(-1);

            return 1;
        }
    }
}