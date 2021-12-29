using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    public class TestRoom : Room
    {
        private readonly UdpServer _udpServer;

        public TestRoom(Guid id, string name, UdpServer udpServer) : base(id, name)
        {
            this._udpServer = udpServer;
        }

        public override async void OnNext(ReceiveData value)
        {
            ITcpCommand tcpCommand = value.TcpCommand;
            switch (tcpCommand)
            {
                case Join x:
                    Add(x.Guid, value.Client);
                    var array = SocketHolder
                        .Where(x => x.Value.udpEndPoint.Port != -1)
                        .Select(x => x.Value.udpEndPoint).ToArray();
                    
                    EndPointPacketHolder packetHolder = new EndPointPacketHolder(array);
                    await UdpSend(_udpServer, packetHolder);
                    break;
                case Leave x:
                    Remove(x.Guid);
                    break;
                case ChatPacket x:
                    // 受信したことのロギング
                    Console.WriteLine($"[CLIENT]{x.Message} ({x.Id.ToString()})");
                    // Tcpで送信
                    await TcpSend(tcpCommand);
                    break;
                default:
                    break;
            }
        }
    }
}