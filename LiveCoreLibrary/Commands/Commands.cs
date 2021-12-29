using System;
using System.Net.Sockets;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [Union(0, typeof(ChatPacket))]
    [Union(1, typeof(Join))]
    [Union(2, typeof(Leave))]
    public interface ITcpCommand { }

    [Union(0, typeof(PositionPacket))]
    [Union(1, typeof(EndPointPacket))]
    [Union(2, typeof(EndPointPacketHolder))]
    //[Union(1, typeof(EndPointPacketHolder))]
    public interface IUdpCommand { }
}