using System;
using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class SetMusicValue
    {
        [Key(0)] public int year;
        [Key(1)] public int month;
        [Key(2)] public int day;
        [Key(3)] public int hour;
        [Key(4)] public int minute;
        [Key(5)] public int seconds;

        public SetMusicValue(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int seconds)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.seconds = seconds;
        }
    }
}