using System;
using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class MessageType
    {
        [Key(0)] public MethodType methodType { get; }
        [Key(1)] public Type type { get; }

        public MessageType(MethodType methodType, Type type)
        {
            this.methodType = methodType;
            this.type = type;
        }
    }
}