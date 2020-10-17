using System;
using System.Threading.Tasks;
using TestServer;

namespace Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Environment.SetEnvironmentVariable("DOTNETQUIC_TRACE", "1");
            
            await SimpleClientAndServer.Run();
        }
    }
}
