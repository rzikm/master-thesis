using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    internal class InProcRunner
    {
        public static int Run(InProcOptions opts, CancellationToken cancellationToken)
        {
            var ep = opts.Tcp
                ? ServerListener.StartTcpTls(new IPEndPoint(IPAddress.Loopback, 0), opts.CertificateFile, opts.PrivateKeyFile, cancellationToken)
                : ServerListener.StartQuic(new IPEndPoint(IPAddress.Loopback, 0), opts.CertificateFile, opts.PrivateKeyFile, cancellationToken);
            
            var clients = Client.StartClients(ep, opts, cancellationToken);
            ResultMonitor.MonitorResults(clients, opts, cancellationToken);
            Task.WaitAll(clients.Select(c => c.Close()).ToArray());
            
            return 0;
        }
    }
}