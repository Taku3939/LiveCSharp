using System;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class ChatPacket : ITcpCommand
    {
        [Key(0)] public ulong Id { get; set; }
        [Key(1)] public string Message { get; set; }

        public ChatPacket(ulong id, string message)
        {
            Id = id;
            Message = message;
        }
    }

}