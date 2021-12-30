using System;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    static class Program
    {
        private const int TcpPort = 30000;
        private const int UdpPort = 7000;

        private static void Main(string[] args)
        {
            //適当なIDを生成
            //Web認証とかできるならそっちでID管理はあり
            // Guid guid = new Guid();
            var holder = new ConcurrentSocketHolder();
            
            // Serverの作成
            var tcp = new TcpServer(TcpPort, holder);
            var udp = new UdpServer(UdpPort);
            
            // とりあえずRoomを一つ作成する
            var room = new P2PChatRoom("Test", udp);

            RoomHolder roomHolder = new RoomHolder();
            roomHolder.Add(room);
            //メッセージの購読
            tcp.Subscribe(roomHolder);
            tcp.Subscribe(room);
            udp.Subscribe(room);
            
            tcp.AcceptLoop(100);
            tcp.HealthCheck(100);
            tcp.ReceiveLoop(10);
            tcp.Process(100);
          
            udp.ReceiveLoop(10);
            udp.Update(100);
            

            while (true)
            {
                var line = Console.ReadLine();

                if (line == "quit")
                {
                    tcp.Close();
                    return;
                }
            }
        }
    }
}