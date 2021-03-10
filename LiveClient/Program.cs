using System;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;
using UniRx;

namespace LiveClient
{
    class Program
    {
        private static Client client;
        private static string host = "localhost";
        private static int port = 30000;

        private static async Task Main(string[] args)
        {
            client = new Client();
            await client.Connect(host, port);
            
            //チャットを受け取ったときのイベント登録
            client.OnMessageReceived?
                .Where(e => e.Item1.type == typeof(ChatMessage))
                .Subscribe(e => Console.WriteLine(MessagePackSerializer.Deserialize<ChatMessage>(e.Item2).message));
            
            //受信開始
            client.ReceiveStart();
            Console.WriteLine("名前を入力してください...");
            var name = Console.ReadLine();
            while (true)
            {
                Console.WriteLine("メッセージを入力してください...");
                var r = Console.ReadLine();
                if (r == "quit") break;
                var m = new ChatMessage(new Random().Next(), $"{name} : {r}");
                var buffer = MessageParser.Encode(m);
                client.SendAsync(buffer);
            }

            client.Close();
            Console.WriteLine("終了します.");
        }
    }
}