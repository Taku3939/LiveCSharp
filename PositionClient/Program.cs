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
        private static Client tcp;
        private static string host = "localhost";
        private static int port = 30000;
        private static int udpPort = 7000;

        private static async Task Main(string[] args)
        {
            // 適当なIDの生成
            Guid guid = Guid.NewGuid();

            // クライアントインスタンスの作成
            tcp = new Client();

            //イベント登録
            tcp.OnMessageReceived += OnMessageReceived;
            tcp.OnConnected += OnConnected;
            tcp.OnDisconnected += OnDisconnected;

            // 接続するまで待機
            while (!await tcp.ConnectAsync(host, port)) Console.Write("...");

            Console.WriteLine(); //　改行

            // Udpの開始
            Udp udp = new Udp(guid, new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpPort));
            udp.ReceiveLoop(10);
            udp.Process(10);

            Console.WriteLine("udp通信を開始します");

            ITcpCommand join = new Join(guid);

            while (true)
            {
                if (!tcp.IsConnected)
                {
                    Console.WriteLine("接続が切れたので終了処理を行います");
                    break;
                }
                // var r = Console.ReadLine();
                // if (r == "quit") break;

                var endPoint = udp.GetEndPoint();
                IUdpCommand command = new PositionPacket(guid, 0, 0, 0, 0, 0, 0,0);
                IUdpCommand endPointPacket = new EndPointPacket(guid, endPoint.Address.ToString(), endPoint.Port);
                tcp.SendAsync(join);
                await udp.SendServer(endPointPacket);
                await udp.SendClients(command);
                await Task.Delay(1000);
            }

            udp.Close();
            tcp.Close();
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
            tcp.ReceiveStart(100);
        }

        private static void OnDisconnected()
        {
            Console.WriteLine($"{host}:[{port.ToString()}] disconnect");
        }
    }
}