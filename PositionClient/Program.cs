using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace PositionClient
{
    class Program
    {
        private static Client _client;
        private static string host = "localhost";
        private static int port = 30000;
        private static int udpPort = 7000;

        private static async Task Main(string[] args)
        {
            // クライアントインスタンスの作成
            _client = new Client();

            //イベント登録
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnConnected += OnConnected;
            _client.OnDisconnected += OnDisconnected;

            // 接続するまで待機
            while (!await _client.ConnectAsync(host, port))
            {
                Console.Write("...");
            }
            
            Console.WriteLine(); //　改行

            // Udpの開始
            Udp udp = new Udp(new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpPort));
            udp.ReceiveLoop(10);
            udp.Process(10);
            Console.WriteLine("udp通信を開始します");
            
            //データの入力
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
                ICommand m = new ChatPacket((ulong) id, $"{name} : {r}");
                await udp.SendServer(m);
                await udp.Send(m);
            }
            
            udp.Close();
            _client.Close();
            Console.WriteLine("終了します.");
        }

        private static void OnMessageReceived(ReceiveData receiveData)
        {
            switch (receiveData.Command)
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