using MessagePack;

namespace MessageObject
{
    /// <summary>
    /// サーバーと送受信するエモートのデータ
    /// </summary>
    [MessagePackObject]
    public class EmoteMessage
    {

        [Key(0)] public ulong UserId { get; }
        [Key(1)] public int Key { get; }

        public EmoteMessage(ulong userId, int key)
        {
            this.UserId = userId;
            this.Key = key;
        }
    }
}