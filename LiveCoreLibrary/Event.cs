using System;
using MessagePack;

namespace LiveClient
{
    public abstract class Event<T> 
    {
        public Type GetMessageType() => typeof(T);
        
        public void Invoke(byte[] buffer) 
        {
            var type = MessagePackSerializer.Deserialize<T>(buffer);
            Received(type);
        }

        /// <summary>
        /// メッセージの受信時にコールされる
        /// </summary>
        /// <param name="t"></param>
        protected abstract void Received(T t);
    }
}