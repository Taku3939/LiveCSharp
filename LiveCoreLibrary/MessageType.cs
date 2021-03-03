using System;
using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class MessageType
    {
        [Key(0)] public int MethodType;
        [Key(1)] public Type type;
    }
}