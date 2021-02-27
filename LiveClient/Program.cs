using System;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;

namespace LiveClient
{
    class Program
    {
        private static Client client;
        static string host = "localhost";
        static int port = 30000;
        private static readonly MusicValue musicValue = new MusicValue() {MusicNumber = 2, TimeCode = 22};
        private static readonly MessageType _type = new MessageType(){type = typeof(MusicValue)};
        private static async Task Main(string[] args)
        {
            client = new Client();
            await client.Connect(host, port);
            client.ReceiveStart();
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "send")
                {
                    var buffer =MessageCreator.Create(_type, musicValue);
                    await client.Send(buffer);
                }
                else if (line == "quit")
                    break;
            }
            
            Console.WriteLine("終了します.");
        }
    }
}