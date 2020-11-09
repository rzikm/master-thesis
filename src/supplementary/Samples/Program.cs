using System.Threading.Tasks;

namespace Samples
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Environment.SetEnvironmentVariable("DOTNETQUIC_TRACE", "console");

            await SimpleClientAndServer.Run();
        }
    }
}