using System;
using System.Net;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace ChatClient
{
    static class Program
    {
        private static Tcp _tcp;
        private static string host = "localhost";
        private static int port = 30000;

        private static string roomName = "Test";
        private static async Task Main(string[] args)
        {
            // クライアントインスタンスの作成
            _tcp = new Tcp();

            //イベント登録
            _tcp.OnMessageReceived += OnMessageReceived;
            _tcp.OnConnected += OnConnected;
            _tcp.OnDisconnected += OnDisconnected;
            
            // 接続するまで待機
            while (!await _tcp.ConnectAsync(host, port)) { Console.Write("..."); }
            Console.WriteLine(); //　改行
            await Task.Delay(1000);
            Console.WriteLine("名前を入力してください...");
            var name = Console.ReadLine();
            var id = Guid.NewGuid();
         
            Join join = new Join(id, roomName);
            _tcp.SendAsync(join);
            var r = Console.ReadLine();
        
            ITcpCommand m = new ChatPacket(id, $"{name} : {r}");
          
            while (true)
            {
                if (!_tcp.IsConnected)
                {
                    Console.WriteLine("接続が切れたので終了処理を行います");
                    break;
                }

                _tcp.SendAsync(m);

                await Task.Delay(1000);
            }

            _tcp.Close();
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
            _tcp.ReceiveStart(100);
        }

        private static void OnDisconnected()
        {
            Console.WriteLine($"{host}:[{port.ToString()}] disconnect");
        }
        
    }
}