using MessagePack;
namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class ChatMessage
    {
        
        [Key(0)]
        public ulong UserId { get; }
        [Key(1)]
        public string Message { get; }
        
        public ChatMessage(ulong userId, string message)
        {
            this.UserId = userId;
            this.Message = message;
        }
    }
}