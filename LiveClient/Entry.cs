using System;
using System.Threading.Tasks;
using LiveCoreLibrary.Client;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Messages;

namespace LiveClient
{
    class Entry
    {
        static string tcpHost = "127.0.0.1";
        static string udpHost = "127.0.0.1";
        static int tcpPort = 25565;
        static int udpPort = 25501;
        static string roomName = "Test";
        static Guid userId;

        public static async Task Main(string[] args)
        {
            // IDの生成
            userId = Guid.NewGuid();
            
            // イベントの追加
            LiveNetwork.Instance.OnMessageReceivedUdpEvent += OnMessageReceivedUdpEvent;
            LiveNetwork.Instance.OnMessageReceivedTcpEvent += OnMessageReceivedTcpEvent;
            LiveNetwork.Instance.OnJoinEvent += OnJoin;
            LiveNetwork.Instance.OnLeaveEvent += OnLeave;

            // TCPサーバーに接続
            await LiveNetwork.Instance.ConnectTcp(tcpHost, tcpPort);
            LiveNetwork.Instance.Join(userId, roomName);
     

            while (true)
            {
                var r = Console.ReadLine();
                if (r == "c") Chat("uouo");
                else if (r == "p") await Position();
                else if (r == "h") await LiveNetwork.Instance.HolePunching();
                else if (r == "l") LiveNetwork.Instance.Leave();
                else if (r == "j") LiveNetwork.Instance.Join(userId, roomName);
                

                await Task.Delay(2000);
            }
        }
        
        public static void OnMessageReceivedUdpEvent(IUdpCommand command)
        {
            switch (command)
            {
                case PositionPacket x:
                    Console.WriteLine("Id : " + x.Id);
                    break;
            }
        }

        public static async void OnJoin(Guid id)
        {
            Console.WriteLine("join is " + id);
            
            LiveNetwork.Instance.ConnectUdp(udpHost, udpPort);
            await LiveNetwork.Instance.HolePunching();
        }
        public static void OnLeave(Guid id)
        {
            Console.WriteLine("leave is " + id);
        }
        public static void OnMessageReceivedTcpEvent(ReceiveData message)
        {
            var command = message.TcpCommand;
            switch (command)
            {
                case ChatPacket x:
                    Console.WriteLine(x.Message);
                    break;
            }
        }

        public static async void ReConnect()
        {
            LiveNetwork.Instance.Leave();
            await Task.Delay(2000);
            LiveNetwork.Instance.Close();
            await Task.Delay(2000);
            await LiveNetwork.Instance.ConnectTcp(tcpHost, tcpPort);
            LiveNetwork.Instance.Join(userId, roomName);
            LiveNetwork.Instance.ConnectUdp(udpHost, udpPort);
        }

        public static async Task Position()
        {
            IUdpCommand command = new PositionPacket(userId, 0, 0, 0, 0, 0, 0, 0);
            await LiveNetwork.Instance.SendClients(command);
        }

        public static void Chat(string message)
        {
            ITcpCommand chat = new ChatPacket(userId, message);
            LiveNetwork.Instance.Send(chat);
        }
    }
}