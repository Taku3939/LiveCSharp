using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class EmotePacket : ITcpCommand
    {
        [Key(0)] public ulong UserId { get; set; }
        [Key(1)] public int Key { get; set; }

        public EmotePacket(ulong userId, int key)
        {
            UserId = userId;
            Key = key;
        }
    }

}