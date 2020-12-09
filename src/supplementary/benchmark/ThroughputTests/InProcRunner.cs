using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    internal class InProcRunner
    {
        public static int Run(InProcOptions opts, CancellationTokenSource cancellationSource)
        {
            var (ep, finished) = opts.Tcp
                ? ServerListener.StartTcpTls(new IPEndPoint(IPAddress.Loopback, 0), opts.CertificateFile, opts.PrivateKeyFile, cancellationSource.Token)
                : ServerListener.StartQuic(new IPEndPoint(IPAddress.Loopback, 0), opts.CertificateFile, opts.PrivateKeyFile, cancellationSource.Token);
            
            var clients = Client.StartClients(ep, opts, cancellationSource.Token);
            ResultMonitor.MonitorResults(clients, opts, cancellationSource);
            Task.WaitAll(clients.Select(c => c.CloseAsync()).Concat(new []{finished}).ToArray());
            
            return 0;
        }
    }
}