using System;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

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
            userId = Guid.NewGuid();
            LiveNetwork.Instance.OnMessageReceivedUdpEvent += OnMessageReceivedUdpEvent;
            LiveNetwork.Instance.OnMessageReceivedTcpEvent += OnMessageReceivedTcpEvent;
            LiveNetwork.Instance.OnJoinEvent += OnJoinEvent;
            await LiveNetwork.Instance.ConnectTcp(tcpHost, tcpPort);
            LiveNetwork.Instance.Join(userId, roomName);
            LiveNetwork.Instance.ConnectUdp(udpHost, udpPort);
            
            while (true)
            {
                await Position();
                Chat("uouo");
          
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
        public static async void OnJoinEvent(Guid id)
        {
            Console.WriteLine("join is " + id);
            await LiveNetwork.Instance.HolePunching();
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
        public async void ReConnect()
        {
            LiveNetwork.Instance.Close();
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