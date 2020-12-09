using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    internal class ClientRunner
    {
        public static int Run(ClientOptions opts, CancellationTokenSource cancellationSource)
        {
            var clients = Client.StartClients(opts.EndPoint, opts, cancellationSource.Token);
            ResultMonitor.MonitorResults(clients, opts, cancellationSource);

            Task.WaitAll(clients.Select(c => c.CloseAsync()).ToArray());
            return 1;
        }
    }
}