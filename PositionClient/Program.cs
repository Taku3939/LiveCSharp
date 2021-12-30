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
        private static Tcp _tcp;
        private static string host = "localhost";
        private static int port = 30000;
        private static int udpPort = 7000;
        private static EndPointPacketHolder _p2PClients;
        private static string roomName = "Test";
        private static async Task Main(string[] args)
        {
            // 適当なIDの生成
            Guid userId = Guid.NewGuid();

            // クライアントインスタンスの作成
            _tcp = new Tcp();

            //イベント登録
            _tcp.OnMessageReceived += OnMessageReceived;
            _tcp.OnConnected += OnConnected;
            _tcp.OnDisconnected += OnDisconnected;

            // 接続するまで待機
            while (!await _tcp.ConnectAsync(host, port)) Console.Write("...");

            Console.WriteLine(); //　改行

            // // Udpの開始
            Udp udp = new Udp(userId, new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpPort));
            udp.ReceiveLoop(10);
            udp.Process(10);

            udp.OnMessageReceived += OnMessageReceivedOfUdp;
            
            Console.WriteLine("udp通信を開始します");
            await Task.Delay(1000);
            ITcpCommand join = new Join(userId,roomName);
            _tcp.SendAsync(join);
            ITcpCommand chat = new ChatPacket(userId, "uouo");
            while (true)
            {
                if (!_tcp.IsConnected)
                {
                    Console.WriteLine("接続が切れたので終了処理を行います");
                    break;
                }
                // var r = Console.ReadLine();
                // if (r == "quit") break;

             
                IUdpCommand command = new PositionPacket(userId, 0, 0, 0, 0, 0, 0,0);
                IUdpCommand endPointPacket = new HolePunchingPacket(userId);
              
                _tcp.SendAsync(chat);
                
                await udp.SendServer(endPointPacket);
                if(_p2PClients != null)await udp.SendClients(command, _p2PClients);
                await Task.Delay(2000);
            }

            udp.Close();
            _tcp.Close();
            Console.WriteLine("終了します.");
        }

        private static void OnMessageReceived(ReceiveData receiveData)
        {
            switch (receiveData.TcpCommand)
            {
                case EndPointPacketHolder x:
                    _p2PClients = x;
                    break;
                case ChatPacket x:
                    Console.WriteLine($"[{x.Id.ToString()}]{x.Message}");
                    break;
                default:
                    break;
            }
        }


        public static void OnMessageReceivedOfUdp(IUdpCommand command)
        {
            switch (command)
            {
                // case EndPointPacketHolder x:
                //     _p2PClients = x;
                //     break;
                    //Console.WriteLine("Address : " + x.Address);
                    //break;
                case PositionPacket x:
                    Console.WriteLine("Id : " + x.Id);
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