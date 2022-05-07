using System;
using System.Net.Sockets;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    public class SocketData
    {
        public ulong id { get; }
        public TcpClient tcpClient { get; }
        public EndPointPacket udpEndPoint { get; }

        public SocketData(ulong id, TcpClient tcpClient, EndPointPacket endPointPacket)
        {
            this.id = id;
            this.tcpClient = tcpClient;
            this.udpEndPoint = endPointPacket;
        }

        public SocketData Clone(EndPointPacket endPointPacket)
        {
            var socketData = new SocketData(id, tcpClient, endPointPacket);
            return socketData;
        }
    }
}