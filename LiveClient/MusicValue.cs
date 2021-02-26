using MessagePack;

namespace LiveClient
{
    [MessagePackObject]
    public class MusicValue
    {
        [Key(0)] public double TimeCode;
        [Key(1)] public int MusicNumber;
    }

}