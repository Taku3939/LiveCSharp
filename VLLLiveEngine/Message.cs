#nullable enable
using System;
using MessagePack;

namespace VLLLiveEngine
{
    [MessagePackObject]
    public class Message
    {
        [Key(0)] public Method Method { get; }
        
        /// <summary>
        /// 送信するメッセージのタイプ
        /// </summary>
        [Key(1)] public string MessageTypeContext { get; }
        
        /// <summary>
        /// 受信先のクラスのタイプ
        /// </summary>
        [Key(2)] public string ReceiveTypeContext { get; }
        
        // [IgnoreMember]
        // public Type? MessageType => Type.GetType(MessageTypeContext);
        //
        // [IgnoreMember]
        // public Type? ReceiveType => Type.GetType(ReceiveTypeContext);

       
        public Message(Method method, Type messageType, Type sourceType)
        {
            this.Method = method;
            this.MessageTypeContext = messageType.ToString();
            this.ReceiveTypeContext = sourceType.ToString();
        }
        
        public Message(Method method, string messageTypeContext, string receiveTypeContext)
        {
            this.Method = method;
            this.MessageTypeContext = messageTypeContext;
            this.ReceiveTypeContext = receiveTypeContext;
        }
    }
}