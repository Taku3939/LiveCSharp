using System;
using LiveCoreLibrary.Commands;

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
            ChatHub chatHub = new ChatHub(holder);
            
            server.OnMessageReceived += data =>
            {
                try
                {
                    var cmd = data.Command;
                    switch (cmd)
                    {
                        case ChatMessage x:
                            chatHub.OnMessageReceived(x);
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