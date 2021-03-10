using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class MusicValue
    {
        [Key(0)] public double TimeCode { get; set; }
        [Key(1)] public int MusicNumber { get; }
        [Key(2)] public PlayState State { get; }

        public MusicValue(double timeCode, int musicNumber, PlayState state)
        {
            this.TimeCode = timeCode;
            this.MusicNumber = musicNumber;
            this.State = state;
        }
    }
}