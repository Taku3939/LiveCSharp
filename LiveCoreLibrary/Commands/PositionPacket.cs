using System;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class PositionPacket : IUdpCommand
    {
        [Key(0)] public readonly Guid Id;
        [Key(1)] public readonly float X;
        [Key(2)] public readonly float Y;
        [Key(3)] public readonly float Z;
        [Key(4)] public readonly float Qx;
        [Key(5)] public readonly float Qy;
        [Key(6)] public readonly float Qz;
        [Key(7)] public readonly float Qw;

        public PositionPacket(Guid id, float x, float y, float z, float qx, float qy, float qz, float qw)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Qx = qx;
            Qy = qy;
            Qz = qz;
            Qw = qw;
        }
    }
}