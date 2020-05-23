using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading.Tasks;

namespace TestServer
{
    internal class MsQuicInterop
    {
        /// <summary>
        ///     Runs a client against a `msquicsample -server ...` server from the msquic repo
        /// </summary>
        private static async Task MsQuicSampleClient()
        {
            // port 4567 is hardcoded in msquicsample executable
            var serverAddress = IPEndPoint.Parse("127.0.0.1:4567");

            using QuicConnection connection = new QuicConnection(serverAddress,
                new SslClientAuthenticationOptions()
                {
                    ApplicationProtocols = new List<SslApplicationProtocol>()
                    {
                        // make sure we report the same protocol
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