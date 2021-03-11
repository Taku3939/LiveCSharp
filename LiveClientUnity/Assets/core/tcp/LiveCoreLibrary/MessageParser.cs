using System;
using MessagePack;
using UnityEngine;

namespace LiveCoreLibrary
{
    public static class MessageParser
    {
        /// <summary>
        /// エンコード用
        /// 0 〜 2 VLL
        /// 3 size
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] Encode<T>(T t)
        {
            var _type = new MessageType(MethodType.Post, typeof(T));
            var b_type = MessagePackSerializer.Serialize(_type);
            var b_value = MessagePackSerializer.Serialize<T>(t);
            byte[] protocol = new byte[3] {(byte) 'V', (byte) 'L', (byte) 'L'};
            byte[] typeSize = new byte[1] {(byte) b_type.Length};
            byte[] valueSize = new byte[1] {(byte) b_value.Length};
            byte[] dist = new byte[5 + b_type.Length + b_value.Length];
            int len = 0;
            Buffer.BlockCopy(protocol, 0, dist, len, protocol.Length);
            Buffer.BlockCopy(typeSize, 0, dist, len += protocol.Length, typeSize.Length);
            Buffer.BlockCopy(valueSize, 0, dist, len += typeSize.Length, valueSize.Length);
            Buffer.BlockCopy(b_type, 0, dist, len += valueSize.Length, b_type.Length);
            Buffer.BlockCopy(b_value, 0, dist, len += b_type.Length, b_value.Length);
            return dist;
        }


        public static byte[] EncodeGet(MethodType type)
        {
            var _type = new MessageType(type, typeof(Unit));
            var b_type = MessagePackSerializer.Serialize(_type);
            var b_value = MessagePackSerializer.Serialize(new Unit());
            byte[] protocol = new byte[3] {(byte) 'V', (byte) 'L', (byte) 'L'};
            byte[] size = new byte[1] {(byte) b_type.Length};
            byte[] dist = new byte[5 + b_type.Length + b_value.Length];
            int len = 0;
            Buffer.BlockCopy(protocol, 0, dist, len, protocol.Length);
            Buffer.BlockCopy(size, 0, dist, len += protocol.Length, size.Length);
            Buffer.BlockCopy(b_type, 0, dist, len += size.Length, b_type.Length);
            Buffer.BlockCopy(b_value, 0, dist, len += b_type.Length, b_value.Length);
            return dist;
        }

        public static bool CheckProtocol(byte[] bytes)
        {
            if (bytes.Length <= 5) return false;

            char V = (char) bytes[0];
            char L = (char) bytes[1];
            char L2 = (char) bytes[2];
            if (V == 'V' && L == 'L' && L2 == 'L') return true;
            else return false;
        }

        /// <summary>
        /// デコード用
        /// </summary>
        /// <param name="receiveBytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static byte[] Decode(byte[] receiveBytes, out MessageType type)
        {
            int size = receiveBytes[3];
            int valueSize = receiveBytes[4];
            byte[] b_type = new byte[size];
            byte[] b_value = new byte[valueSize];
            Buffer.BlockCopy(receiveBytes, 5, b_type, 0, b_type.Length);
            Buffer.BlockCopy(receiveBytes, b_type.Length + 5, b_value, 0, b_value.Length);
            MessageType messageType = MessagePackSerializer.Deserialize<MessageType>(b_type);
            type = messageType;
            return b_value;
        }
    }
}