using System;
using MessagePack;

namespace LiveCoreLibrary
{
    public static class MessageParser
    {
        /// <summary>
        /// エンコード用
        /// 0 〜 2 VLL
        /// 3 size
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] Encode<T>(string rest, T t, MethodType mType = MethodType.Post)
        {
            var _type = new MessageType(mType, rest);
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
        /// <param name="rest"></param>
        /// <returns></returns>
        public static byte[] Decode(byte[] receiveBytes, out MessageType rest)
        {
            int size = receiveBytes[3];
            int valueSize = receiveBytes[4];
            byte[] b_type = new byte[size];
            byte[] b_value = new byte[valueSize];
            Buffer.BlockCopy(receiveBytes, 5, b_type, 0, b_type.Length);
            Buffer.BlockCopy(receiveBytes, b_type.Length + 5, b_value, 0, b_value.Length);
            MessageType messageType = MessagePackSerializer.Deserialize<MessageType>(b_type);
            rest = messageType;
            return b_value;
        }
        
        /// <summary>
        /// デコード用
        /// </summary>
        /// <param name="receiveBytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T DecodeBody<T>(byte[] receiveBytes)
        {
            int size = receiveBytes[3];
            int valueSize = receiveBytes[4];
            byte[] b_value = new byte[valueSize];
            Buffer.BlockCopy(receiveBytes, size + 5, b_value, 0, b_value.Length);
            T value = MessagePackSerializer.Deserialize<T>(b_value);
            
            return value;
        }

        public static MessageType DecodeType(byte[] receiveBytes)
        {
            int size = receiveBytes[3];
            byte[] b_type = new byte[size];
            Buffer.BlockCopy(receiveBytes, 5, b_type, 0, size);
            MessageType messageType = MessagePackSerializer.Deserialize<MessageType>(b_type);
            return messageType;
        }
    }
}