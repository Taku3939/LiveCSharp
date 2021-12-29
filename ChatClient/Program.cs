using System;
using System.Net;
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
            // クライアントインスタンスの作成
            _client = new Client();

            //イベント登録
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnConnected += OnConnected;
            _client.OnDisconnected += OnDisconnected;
            
            // 接続するまで待機
            while (!await _client.ConnectAsync(host, port)) { Console.Write("..."); }
            Console.WriteLine(); //　改行

            Console.WriteLine("名前を入力してください...");
            var name = Console.ReadLine();
            var id = new Random().Next();
            Console.WriteLine("メッセージを入力してください...");
            while (true)
            {
                if (!_client.IsConnected)
                {
                    Console.WriteLine("接続が切れたので終了処理を行います");
                    break;
                }
                var r = Console.ReadLine();
                if (r == "quit") break;
                ITcpCommand m = new ChatPacket((ulong) id, $"{name} : {r}");
                _client.SendAsync(m);
            }

            _client.Close();
            Console.WriteLine("終了します.");
        }

        private static void OnMessageReceived(ReceiveData receiveData)
        {
            switch (receiveData.TcpCommand)
            {
                case ChatPacket x:
                    Console.WriteLine($"[{x.Id.ToString()}]{x.Message}");
                    break;
                default:
                    break;
            }
        }
        private static void OnConnected()
        {
            //受信開始
            Console.WriteLine($"{host}:[{port.ToString()}] connect");
            Console.WriteLine("--------------");
            _client.ReceiveStart(100);
        }

        private static void OnDisconnected()
        {
            Console.WriteLine($"{host}:[{port.ToString()}] disconnect");
        }
        
    }
}