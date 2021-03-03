using MessagePack;
namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class ChatMessage
    {
        [Key(0)] public string id;
        [Key(1)] public string message;
    }
}