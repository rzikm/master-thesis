using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThroughputTests
{
    internal class ClientRunner
    {
        public static int Run(ClientOptions opts, CancellationToken cancellationToken)
        {
            var clients = Client.StartClients(opts.Connections, opts.MessageSize, opts.EndPoint, opts.Streams, cancellationToken);
            ResultMonitor.MonitorResults(clients, opts, cancellationToken);

            Task.WaitAll(clients.Select(c => c.Close()).ToArray());
            return 1;
        }
    }
}