using System;
using System.Threading.Tasks;
using LiveCoreLibrary;
using UniRx;

namespace LiveClient
{
    class Program
    {
        private static Client client;
        static string host = "localhost";
        static int port = 30000;
        //private static readonly MusicValue musicValue = new MusicValue() {MusicNumber = 2, TimeCode = 22, message = "uouo"};
        private static readonly MessageType _type = new MessageType() {type = typeof(MusicValue)};
        private static TestEvent _event = new TestEvent();

        private static async Task Main(string[] args)
        {
            client = new Client();
            await client.Connect(host, port);

            client.OnMessageReceived
                .Where(e => e.Item1.type == _event.GetMessageType())
                .Subscribe(e => _event.Invoke(e.Item2));

            client.ReceiveStart();
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "send")
                {
                    Console.WriteLine("メッセージを入力してください...");
                    var r = Console.ReadLine();
                    var m = new MusicValue() {MusicNumber = 2, TimeCode = 22, message = r};
                    var buffer = MessageParser.Encode(_type, m);
                    await client.Send(buffer);
                }
                else if (line == "quit")
                    break;
            }

            Console.WriteLine("終了します.");
        }
    }
}