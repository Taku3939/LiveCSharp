using System;
using MessagePack;

namespace LiveCoreLibrary
{
    [Serializable]
    [MessagePackObject]
    public class MusicValue
    {
        [Key(0)] public double StartTimeCode;
        
        [Key(1)] public double CurrentTime;
        public MusicValue(double startTimeCode, double currentTime)
        {
            this.StartTimeCode = startTimeCode;
            this.CurrentTime = currentTime;
        }
    }
}