using System;
using System.Threading.Tasks;
using TestServer;

namespace Samples
{
    class Program
    {
        static Task Main(string[] args)
        {
            return SimpleClientAndServer.Run();
        }
    }
}
