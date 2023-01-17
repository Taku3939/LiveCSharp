using System;
using System.Net.Sockets;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [Union(0, typeof(ChatPacket))]
    [Union(1, typeof(Join))]
    [Union(2, typeof(Leave))]
    [Union(3, typeof(JoinResult))]
    [Union(4, typeof(LeaveResult))]
    [Union(5, typeof(EndPointPacketHolder))]
    [Union(6, typeof(EmotePacket))]
    [Union(7, typeof(ColorPacket))]
    public interface ITcpCommand { }

    [Union(0, typeof(PositionPacket))]
    [Union(1, typeof(HolePunchingPacket))]
    
    //[Union(1, typeof(EndPointPacketHolder))]
    public interface IUdpCommand { }
}