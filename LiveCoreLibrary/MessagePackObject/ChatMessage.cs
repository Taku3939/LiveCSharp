using MessagePack;
namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)] public long id { get; }
        [Key(1)] public string message { get; }
        public ChatMessage(long id, string message)
        {
            this.id = id;
            this.message = message;
        }
    }
}