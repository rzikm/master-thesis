﻿using System;
using System.Net;
using System.Net.Quic.Public;
using System.Net.Security;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

namespace TestServer
{
    class Program
    {
        private const int WindowSize = 4 * 1024;
        private const int WindowCount = 10;
        private const int DataSize = 128 * 1024 * 1024;

        static IPEndPoint GetEndpoint(string host, int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            return new IPEndPoint(ipAddress, port);
        }

        static IPEndPoint serverEndpoint = GetEndpoint("localhost", 5000);

        static async void StartServer()
        {
            QuicListenerOptions options = new QuicListenerOptions()
            {
                CertificateFilePath = "Certs/cert.crt",
                PrivateKeyFilePath = "Certs/cert.key",
                ListenEndPoint = serverEndpoint
            };

            QuicListener listener = new QuicListener(options);

            Console.WriteLine($@"Starting listener on {serverEndpoint}");
            listener.Start();

            Console.WriteLine("Waiting for incoming connection...");

            var connection = await listener.AcceptConnectionAsync();
            Console.WriteLine("Connection accepted, openning stream");
            var stream = connection.OpenBidirectionalStream();

            Console.WriteLine($"Writing {DataSize} bytes of data");
            byte[] buffer = new byte[1024];
            byte[] discard = new byte[1024];

            // write known data so that we can assert it on the other size
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) i;
            }

            int maxWrite = WindowSize;
            int written = 0;
            int windows = 1;

            while (written < DataSize)
            {
                while (written < maxWrite)
                {
                    stream.Write(buffer);
                    written += buffer.Length;
                }

                windows += await stream.ReadAsync(discard);
                maxWrite = windows * WindowSize;
            }

            Console.WriteLine("Data written");
        }

        static async Task Main(string[] args)
        {
            StartServer();

            Console.WriteLine("Creating client connection");
            var client = new QuicConnection(serverEndpoint, null);
            
            Console.WriteLine("Connecting to the server");
            await client.ConnectAsync();

            Console.WriteLine("Client connected, waiting for stream.");
            var stream = await client.AcceptStreamAsync();

            Console.WriteLine("Stream received, pulling data");
            byte[] buffer = new byte[1024 * 4];

            int received = 0;
            int nextWindow = WindowSize;
            while (received < DataSize)
            {
                int recv = await stream.ReadAsync(buffer);

                for (int i = 0; i < recv; i++)
                {
                    if (buffer[i] != (byte) (received + i))
                    {
                        throw new InvalidOperationException("Data corrupted");
                    }
                }

                received += recv;
                nextWindow -= recv;
                if (nextWindow <= 10 * WindowSize)
                {
                    stream.Write(buffer, 0, 1);
                    nextWindow += WindowSize;
                }
            }

            Console.WriteLine($"Received all bytes");
        }
    }
}