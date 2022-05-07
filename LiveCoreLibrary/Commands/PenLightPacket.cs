using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class PenLightPacket : ITcpCommand
    {
        [Key(0)] public ulong UserId { get; set; }
        [Key(1)] public float R { get; set; }
        [Key(2)] public float G { get; set; }
        [Key(3)] public float B { get; set; }
        public PenLightPacket(ulong userId, float r, float g, float b)
        {
            UserId = userId;
            R = r;
            G = g;
            B = b;
        }

    }

}