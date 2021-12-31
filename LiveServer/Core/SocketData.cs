using System;
using System.Net.Sockets;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    public class SocketData
    {
        public Guid Guid { get; }
        public TcpClient tcpClient { get; }
        public EndPointPacket udpEndPoint { get; }

        public SocketData(Guid guid, TcpClient tcpClient, EndPointPacket endPointPacket)
        {
            this.Guid = guid;
            this.tcpClient = tcpClient;
            this.udpEndPoint = endPointPacket;
        }

        public SocketData Clone(EndPointPacket endPointPacket)
        {
            var socketData = new SocketData(Guid, tcpClient, endPointPacket);
            return socketData;
        }
    }
}