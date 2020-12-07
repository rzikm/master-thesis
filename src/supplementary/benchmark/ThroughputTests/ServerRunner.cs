using System;
using System.Net;
using System.Threading;

namespace ThroughputTests
{
    internal class ServerRunner
    {
        public static int Run(ServerOptions opts, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Running QUIC server on {opts.EndPoint}");
            ServerListener.StartQuic(opts.EndPoint, opts.CertificateFile, opts.PrivateKeyFile, cancellationToken);
            
            IPEndPoint sslEndpoint = new IPEndPoint(opts.EndPoint.Address, opts.EndPoint.Port + 1);
            Console.WriteLine($"Running SSL server on {sslEndpoint}");
            ServerListener.StartTcpTls(sslEndpoint, opts.CertificateFile, opts.PrivateKeyFile, cancellationToken);
            
            Thread.Sleep(-1);

            return 1;
        }
    }
}