using System;
using System.Threading.Tasks;
using MessageObject;
using MessagePack;
using UniRx;
using VLLLiveEngine;
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
            client.OnMessageReceived?
                .Where(e => e.Item1.MessageTypeContext == typeof(ChatMessage).ToString())
                .Subscribe(e => Console.WriteLine(MessagePackSerializer.Deserialize<ChatMessage>(e.Item2).message));
            
            //受信開始
            client.ReceiveStart(100);
            Console.WriteLine("名前を入力してください...");

            ChatHub chatHub = new ChatHub(client);
            var name = Console.ReadLine();
            var id = (ulong) new Random().Next();
            while (true)
            {
                Console.WriteLine("メッセージを入力してください...");
                var r = Console.ReadLine();
                if (r == "quit") break;
                var m = new ChatMessage(id, $"{name} : {r}");
                chatHub.Send(m);
            }

            client.Close();
            Console.WriteLine("終了します.");
        }
    }
}