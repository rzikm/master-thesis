using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TestServer.QuicTracer;

namespace TestServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var eventListener = new QuicEventListener();
            var logger = new QuicPacketLogger(eventListener.EventReader);
            logger.Start();
            // Environment.SetEnvironmentVariable("USE_MSQUIC", "1");

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

            // TODO: probably need ALPN to interop with msquic
            await connection.ConnectAsync();

            await using var stream = connection.OpenBidirectionalStream();

            byte[] buffer = new byte[1024];
            new Random().NextBytes(buffer);
            await stream.WriteAsync(buffer);
            await stream.ShutdownWriteCompleted();

            byte[] recv = new byte[1024];
            int read = await stream.ReadAsync(buffer);

            Console.WriteLine($"Received: {BitConverter.ToString(buffer, 0, read)}");
        }
    }
}