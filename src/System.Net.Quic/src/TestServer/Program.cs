using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Threading.Channels;
using System.Threading.Tasks;
using TestServer.QuicTracer;

namespace TestServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            // var eventListener = new QuicEventListener();
            // var logger = new QuicPacketLogger(eventListener.EventReader);
            // var logTask = Task.Run(logger.Start);
            // Environment.SetEnvironmentVariable("USE_MSQUIC", "1");
            await Sample.Run();
            // await Run();
            // eventListener.Stop();
            // await logTask;
        }
    }
}