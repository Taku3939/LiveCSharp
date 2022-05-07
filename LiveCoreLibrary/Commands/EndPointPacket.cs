using System;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class HolePunchingPacket : IUdpCommand
    {
        [Key(0)] public readonly ulong UserId;

        public HolePunchingPacket(ulong userId)
        {
            this.UserId = userId;
        }
    }

    [MessagePackObject]
    public readonly struct EndPointPacket : IEquatable<EndPointPacket>
    {
        [Key(0)] public readonly ulong Id ;
        [Key(1)] public readonly string Address ;
        [Key(2)] public readonly int Port ;

        public EndPointPacket(ulong id, string address, int port)
        {
            Id = id;
            Address = address;
            Port = port;
        }
        public override int GetHashCode()
        {
            int hash = 17; 
            hash= hash * 23 + Id.GetHashCode();
            hash= hash * 23 + Address.GetHashCode();
            hash= hash * 23 + Port.GetHashCode();
            return hash;
        }
        public override bool Equals(object other)
        {
            if (other is EndPointPacket)
                return Equals((EndPointPacket)other);
            return false;
        }

        public bool Equals(EndPointPacket other)
        {
            return Id == other.Id &&
                   Address == other.Address &&
                   Port == other.Port;
        }

        public static bool operator ==(EndPointPacket lhs, EndPointPacket rhs)
        {
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(EndPointPacket lhs, EndPointPacket rhs) => !(lhs == rhs);
    }

    [MessagePackObject]
    public class EndPointPacketHolder : ITcpCommand
    {
        [Key(0)] public readonly EndPointPacket[] EndPointPackets;

        public EndPointPacketHolder(EndPointPacket[] endPointPackets)
        {
            this.EndPointPackets = endPointPackets;
        }
    }
}