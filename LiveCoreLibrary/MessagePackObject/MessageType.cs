using System;
using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class MessageType
    {
        [Key(0)] public MethodType methodType { get; }
        [Key(1)] public string rest { get; }

        public MessageType(MethodType methodType, string rest)
        {
            this.methodType = methodType;
            this.rest = rest;
        }
    }
}