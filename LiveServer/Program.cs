using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary;

namespace LiveServer
{
    class Program
    {
        const int Port = 30000;

        private static void Main(string[] args)
        {
            var holder = new ConcurrentSocketHolder();

            Server server = new Server(Port, holder);
            server.AcceptLoop(100);
            server.HealthCheck(100);
            server.ReceiveLoop(10);

            var hub = new MusicHub(holder);
            server.OnMessageReceived += async (args) =>
            {
                try
                {
                    var body = MessageParser.Decode(args.Item2, out var rest);
                    switch (rest.rest)
                    {
                        case REST.MUSIC_SET_VALUE:
                            hub.SetTime(MessageParser.DecodeBody<SetMusicValue>(body));
                            break;

                        // case "/m/update":
                        //     hub.UpdateTime(MessageParser.DecodeBody<MusicValue>(body));
                        //     break;

                        case REST.MUSIC_GET_VALUE:
                            hub.GetTime(args.Item3);
                            break;
                        
                        default:
                            break;
                    }

                    if (rest.methodType == MethodType.Post)
                    {
                        foreach (var client in holder.GetClients())
                            await client.Client.SendAsync(args.Item2, SocketFlags.None);
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