using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class MusicValue
    {
        [Key(0)] public double TimeCode;
        [Key(1)] public int MusicNumber;
        [Key(2)] public int State;

        // public MusicValue(double timeCode, int musicNumber, int state)
        // {
        //     this.TimeCode = timeCode;
        //     this.MusicNumber = musicNumber;
        //     this.State = state;
        // }
    }
}