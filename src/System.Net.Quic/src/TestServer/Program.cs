using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading.Tasks;
using TestServer.QuicTracer;

namespace TestServer
{
    internal static class Program
    {
        public static Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("USE_MSQUIC", "1");
            return Sample.Run();
        }

        private static async Task MsQuicSample()
        {
            var eventListener = new QuicEventListener();
            var logger = new QuicPacketLogger(eventListener.EventReader);
            logger.Start();
            Environment.SetEnvironmentVariable("USE_MSQUIC", "1");

            // port 4567 is hardcoded in msquic sample 
            var serverAddress = IPEndPoint.Parse("127.0.0.1:4567");

            using QuicConnection connection = new QuicConnection(serverAddress,
                new SslClientAuthenticationOptions()
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>()
                    {
                        new SslApplicationProtocol("sample")
                    }
                });

            await connection.ConnectAsync();

            await using var stream = connection.OpenBidirectionalStream();

            byte[] buffer = new byte[1024];
            new Random().NextBytes(buffer);
            await stream.WriteAsync(buffer);
            await stream.ShutdownWriteCompleted();

            int totalRead = 0;
            int read;
            do
            {
                read = await stream.ReadAsync(buffer.AsMemory(totalRead));
                totalRead += read;
            } while (read > 0);

            Console.WriteLine($"Received: {BitConverter.ToString(buffer, 0, totalRead)}");
        }
    }
}