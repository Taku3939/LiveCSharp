using System;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace ChatClient
{
    static class Program
    {
        private static Client _client;
        private static string host = "localhost";
        private static int port = 30000;

        private static async Task Main(string[] args)
        {
            _client = new Client();
            await _client.ConnectAsync(host, port);

            //チャットを受け取ったときのイベント登録
            _client.OnMessageReceived += (args) =>
            {
                
                switch (args.Command)
                {
                    case Join x:
                        Console.WriteLine(x._content);
                        break;
                    case Remove x:
                        break;
                    case ChatMessage x:
                        Console.WriteLine($"{x.Id.ToString()} : {x.Message}");
                        break;
                    default:
                        break;
                }
            };

            //受信開始
            _client.ReceiveStart(100);
            Console.WriteLine("名前を入力してください...");
            var name = Console.ReadLine();
            var id = new Random().Next();
            Console.WriteLine("メッセージを入力してください...");
            while (true)
            {
               
                var r = Console.ReadLine();
                if (r == "quit") break;
                ICommand m = new ChatMessage((ulong) id, $"{name} : {r}");
                _client.SendAsync(m);
            }

            _client.Close();
            Console.WriteLine("終了します.");
        }
    }
}