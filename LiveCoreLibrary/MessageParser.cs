using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveClient;
using MessagePack;

namespace LiveCoreLibrary
{
    public static class MessageParser
    {
        /// <summary>
        /// エンコード用
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] Encode<T>(MessageType _type, T t)
        {
            var b_type = MessagePackSerializer.Serialize(_type);
            var b_value = MessagePackSerializer.Serialize<T>(t);
            byte[] size = new byte[1] {(byte) b_type.Length};
            byte[] dist = new byte[1 + b_type.Length + b_value.Length];
            Buffer.BlockCopy(size, 0, dist, 0, size.Length);
            Buffer.BlockCopy(b_type, 0, dist, size.Length, b_type.Length);
            Buffer.BlockCopy(b_value,0, dist,size.Length + b_type.Length, b_value.Length);
            return dist;
        }
        
        /// <summary>
        /// デコード用
        /// </summary>
        /// <param name="receiveBytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static byte[] Decode(byte[] receiveBytes, out MessageType type)
        {
            int size = receiveBytes[0];
            byte[] b_type = new byte[size];
            byte[] b_value = new byte[receiveBytes.Length - size - 1];
            Buffer.BlockCopy(receiveBytes, 1, b_type, 0, b_type.Length);
            Buffer.BlockCopy(receiveBytes, b_type.Length + 1, b_value, 0, b_value.Length);
            MessageType messageType = MessagePackSerializer.Deserialize<MessageType>(b_type);
            type = messageType;
            return b_value;
        }
    }
}