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
            Server server = new Server(port, new SocketHolder());
            server.AcceptLoop();
            server.HealthCheck();
            server.ReceiveLoop();
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