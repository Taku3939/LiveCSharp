using System;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public struct EndPointPacket : IUdpCommand
    {
        [Key(0)] public readonly Guid Guid;
        [Key(1)] public readonly string Address;
        [Key(2)] public readonly int Port;

        public EndPointPacket(Guid guid, string address, int port)
        {
            Guid = guid;
            Address = address;
            Port = port;
        }
    }

    [MessagePackObject]
    public class EndPointPacketHolder : IUdpCommand
    {
        [Key(0)] public readonly EndPointPacket[] EndPointPackets;

        public EndPointPacketHolder(EndPointPacket[] endPointPackets)
        {
            this.EndPointPackets = endPointPackets;
        }
    }
}