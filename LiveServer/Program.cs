using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

namespace LiveServer
{
    class Program
    {
        private static void Main(string[] args)
        {
            int port = 30000;
            Server server = new Server(port, new ConcurrentSocketHolder());
            server.AcceptLoop(0);
            server.HealthCheck(10);
            server.ReceiveLoop(0);
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "quit")
                {
                    server.Close();
                    return;
                }
            }
        }
    }
}