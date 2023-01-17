using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class ColorPacket : ITcpCommand
    {
        [Key(0)] public ulong Id { get; set; }
        [Key(1)] public float R { get; set; }
        [Key(2)] public float G { get; set; }
        [Key(3)] public float B { get; set; }

        public ColorPacket(ulong id, float r, float g, float b)
        {
            Id = id;
            R = r;
            G = g;
            B = b;

        }
    }

}