using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    public class P2PChatRoom : P2PRoom
    {
        public P2PChatRoom(string name, UdpServer udpServer) : base(name, udpServer) { }

        public override async void OnReceivedTcp(ReceiveData value)
        {
            ITcpCommand tcpCommand = value.TcpCommand;
            switch (tcpCommand)
            {
                case ChatPacket x:
                    // 受信したことのロギング
                    Console.WriteLine($"[CLIENT]{x.Message} ({x.Id.ToString()})");
                    // Tcpで送信
                    await TcpSend(tcpCommand);
                    break;
            }
        }
    }
}