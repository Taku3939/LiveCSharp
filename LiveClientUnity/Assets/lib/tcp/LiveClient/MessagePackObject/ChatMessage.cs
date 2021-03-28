using MessagePack;
namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)] public string username { get; }
        [Key(1)] public string message { get; }

        public ChatMessage(string username, string message)
        {
            this.username = username;
            this.message = message;
        }
    }
}
