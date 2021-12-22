using System;
using System.Diagnostics;
using System.Net.Sockets;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using MessagePack;

namespace LiveServer
{
    static class Program
    {
        const int Port = 30000;

        private static void Main(string[] args)
        {
            var holder = new ConcurrentSocketHolder();

            Server server = new Server(Port, holder);
            server.AcceptLoop(100);
            server.HealthCheck(100);
            server.ReceiveLoop(10);

            server.OnMessageReceived += async (args) =>
            {
                try
                {
                    var data = args.Command;
                    // var tcp = args.Item2; 
                    switch (data)
                    {
                        case ChatMessage x:
                            Console.WriteLine($"[CLIENT]{x.Message} ({x.Id.ToString()})");
                            foreach (var client in holder.GetClients())
                            {
                                await client.Client.SendAsync(MessageParser.Encode(x), SocketFlags.None);
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            };

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