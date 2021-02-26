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
        static async Task Main(string[] args)
        {
            int port = 30000;
            Server server = new Server(port);
            server.AcceptLoop();
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "quit")
                {
                    return;
                }
            }
        }
    }
}