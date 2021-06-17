using System;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;

namespace ChatClient
{
    class Program
    {
        private static Client client;
        private static string host = "localhost";
        private static int port = 30000;

        private static async Task Main(string[] args)
        {
            client = new Client();
            await client.ConnectAsync(host, port);

            //チャットを受け取ったときのイベント登録
            client.OnMessageReceived += (args) =>
            {
                var body = MessageParser.Decode(args.Item2, out var rest);
                switch(rest.rest) 
                {
                    case "/c/send":
                        Console.WriteLine(MessagePackSerializer.Deserialize<ChatMessage>(body).Message);
                        break;
                };
            };

            //受信開始
            client.ReceiveStart(100);
            Console.WriteLine("名前を入力してください...");
            var name = Console.ReadLine();
            while (true)
            {
                Console.WriteLine("メッセージを入力してください...");
                var r = Console.ReadLine();
                if (r == "quit") break;
                var m = new ChatMessage((ulong) new Random().Next(), $"{name} : {r}");
                var buffer = MessageParser.Encode("/c/send", m);
                client.SendAsync(buffer);
            }

            client.Close();
            Console.WriteLine("終了します.");
        }
    }
}