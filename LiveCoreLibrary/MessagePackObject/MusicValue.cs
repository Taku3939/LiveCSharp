using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class MusicValue
    {
        /// <summary>
        /// 曲を開始する時間
        /// </summary>
        [Key(0)] public double StartTimeCode { get; set; }
        [Key(1)] public int MusicNumber { get; }
        [Key(2)] public PlayState State { get; }

        public MusicValue(double startTimeCode, int musicNumber, PlayState state)
        {
            this.StartTimeCode = startTimeCode;
            this.MusicNumber = musicNumber;
            this.State = state;
        }
    }
}