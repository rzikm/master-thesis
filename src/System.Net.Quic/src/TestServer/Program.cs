using System;
using System.Net;
using System.Net.Quic.Public;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TestServer
{
    class TestSockets
    {
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
            Console.WriteLine($@"Starting listener");
            TcpListener listener = new TcpListener(serverEndpoint);
            listener.Start();
            Console.WriteLine($@"listening on {listener.LocalEndpoint}");
            await Task.Yield();

            Console.WriteLine("Waiting for incoming connection...");
            var connection = await listener.AcceptSocketAsync();
            Console.WriteLine($"Connection accepted, Writing {DataSize} bytes of data");
            byte[] buffer = new byte[1024 * 16];

            // write known data so that we can assert it on the other size
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) i;
            }

            int written = 0;

            while (written < DataSize)
            {
                await connection.SendAsync(buffer, SocketFlags.None);
                written += buffer.Length;
            }
            
            connection.Close();
            connection.Dispose();

            Console.WriteLine("Data written");
        }

        public static async Task Run(string[] args)
        {
            StartServer();

            Console.WriteLine("Creating client connection");
            var client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            
            Console.WriteLine("Connecting to the server");
            client.Connect(serverEndpoint);

            Console.WriteLine("Client connected, pulling data");
            byte[] buffer = new byte[1024 * 16];

            int received = 0;
            while (received < DataSize)
            {
                int recv = await client.ReceiveAsync(buffer, SocketFlags.None);

                for (int i = 0; i < recv; i++)
                {
                    if (buffer[i] != (byte) (received + i))
                    {
                        throw new InvalidOperationException("Data corrupted");
                    }
                }

                received += recv;
            }

            Console.WriteLine($"Received all bytes");
        }

    }
    
    class Program
    {
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

            Console.WriteLine($@"Starting listener");
            using QuicListener listener = new QuicListener(options);

            listener.Start();
            Console.WriteLine($@"listening on {listener.ListenEndPoint}");

            Console.WriteLine("Waiting for incoming connection...");

            using var connection = await listener.AcceptConnectionAsync();
            Console.WriteLine("Connection accepted, opening stream");
            await using var stream = connection.OpenBidirectionalStream();

            Console.WriteLine($"Writing {DataSize} bytes of data");
            byte[] buffer = new byte[1024 * 16];

            // write known data so that we can assert it on the other size
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte) i;
            }

            int written = 0;

            while (written < DataSize)
            {
                await stream.WriteAsync(buffer);
                written += buffer.Length;
            }

            stream.Shutdown();
            await stream.FlushAsync();
            
            Console.WriteLine("Data written, closing.");
            
            await stream.ShutdownWriteCompleted();
            Console.WriteLine("Server closed");
        }

        static async Task Main(string[] args)
        {
            // await TestSockets.Run(args);
            // return;
            StartServer();

            Console.WriteLine("Creating client connection");
            using var client = new QuicConnection(serverEndpoint, null);
            
            Console.WriteLine("Connecting to the server");
            await client.ConnectAsync();

            Console.WriteLine("Client connected, waiting for stream.");
            await using var stream = await client.AcceptStreamAsync();

            Console.WriteLine("Stream received, pulling data");
            byte[] buffer = new byte[1024 * 16];

            int received = 0;
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
            }

            Console.WriteLine($"Received all bytes");
        }
    }
}