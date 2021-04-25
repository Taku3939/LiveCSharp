using MessagePack;
namespace MessageObject
{
    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)] public ulong id { get; }
        [Key(1)] public string message { get; }
        public ChatMessage(ulong id, string message)
        {
            this.id = id;
            this.message = message;
        }
    }
}