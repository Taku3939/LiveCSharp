using MessagePack;

namespace LiveCoreLibrary
{
    /// <summary>
    /// サーバーと送受信するエモートのデータ
    /// </summary>
    [MessagePackObject]
    public class EmoteMessage
    {
        [Key(0)] public int key { get; }

        public EmoteMessage(int key)
        {
            this.key = key;
        }
    }
}