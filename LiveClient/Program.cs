using System;
using System.Security.Claims;
using System.Threading;
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

        private float timeStamp;

        private static async Task Main(string[] args)
        {
            client = new Client();
            await client.ConnectAsync(host, port);
            
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
    public class MusicSync
    {
        private Client _client;
        private double TimeStamp;

        public MusicSync()
        {
            _client = new Client();
            _client.ConnectAsync("127.0.0.1", 30000);
            _client.OnMessageReceived
                .Where(x => x.Item1.type == typeof(MusicValue))
                .Subscribe(x =>
                {
                    var value = MessageParser.Decode<MusicValue>(x.Item2);
                });
        }
        void Send()
        {
            //HOST
            //送信時にタイムスタンプを保存する
            var dateTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            MusicValue value = new MusicValue(dateTime, 1, PlayState.Playing);
            _client.SendAsync(value);
        }        
    }
}