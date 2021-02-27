using System;
using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class MessageType
    {
        [Key(0)]
        public Type type;
    }
}