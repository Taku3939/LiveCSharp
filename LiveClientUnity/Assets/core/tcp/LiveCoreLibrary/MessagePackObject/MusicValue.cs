using System;
using MessagePack;

namespace LiveCoreLibrary
{
    [Serializable]
    [MessagePackObject]
    public class MusicValue
    {
        [Key(0)] public double TimeCode { get; }
        [Key(1)] public int MusicNumber { get; }
        [Key(2)] public int State { get; }

        public MusicValue(double timeCode, int musicNumber, int state)
        {
            this.TimeCode = timeCode;
            this.MusicNumber = musicNumber;
            this.State = state;
        }
    }
}