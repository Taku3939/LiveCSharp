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
            
            // TcpServerの作成
            TcpServer tcpTcpServer = new TcpServer(TcpPort, holder);
            tcpTcpServer.AcceptLoop(100);
            tcpTcpServer.HealthCheck(100);
            tcpTcpServer.ReceiveLoop(10);
            tcpTcpServer.Process(100);
            
            var udpServer = new UdpServer(UdpPort);
            udpServer.ReceiveLoop(10);
            udpServer.Update(100);
            
            // とりあえずRoomを一つ作成する
            Room room = new TestRoom(Guid.NewGuid(), "TestRoom", udpServer);
            
            // メッセージの購読
           
            tcpTcpServer.Subscribe(room);
            udpServer.Subscribe(room);
            while (true)
            {
                var line = Console.ReadLine();

                if (line == "quit")
                {
                    tcpTcpServer.Close();
                    return;
                }
            }
        }
    }
}