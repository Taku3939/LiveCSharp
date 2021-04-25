using System;
using MessagePack;

namespace MessageObject
{
    [Serializable]
    [MessagePackObject]
    public class MusicValue
    {
        [Key(0)] public double StartTimeCode { get; }
        public MusicValue(double startTimeCode)
        {
            this.StartTimeCode = startTimeCode;
        }
    }
}