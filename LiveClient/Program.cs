using System;
using System.Threading.Tasks;
using LiveCoreLibrary;
using UniRx;

namespace LiveClient
{
    class Program
    {
        private static Client client;
        private static string host = "localhost";
        private static int port = 30000;
        private static readonly TestEvent Event = new TestEvent();

        private static async Task Main(string[] args)
        {
            client = new Client();
            await client.Connect(host, port);

            client.OnMessageReceived
                .Where(e => e.Item1.type == Event.GetMessageType())
                .Subscribe(e => Event.Invoke(e.Item2));

            client.ReceiveStart();
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "send")
                {
                    Console.WriteLine("メッセージを入力してください...");
                    var r = Console.ReadLine();
                    var m = new MusicValue() {MusicNumber = 2, TimeCode = 22, State = 0};
                    var buffer = MessageParser.Encode(m);
                    await client.Send(buffer);
                }
                else if (line == "quit")
                    break;
            }

            Console.WriteLine("終了します.");
        }
    }
}