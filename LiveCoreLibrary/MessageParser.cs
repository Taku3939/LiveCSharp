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
        /// 0 〜 2 VLL
        /// 3 size
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] Encode<T>(MessageType _type, T t)
        {
         
            var b_type = MessagePackSerializer.Serialize(_type);
            var b_value = MessagePackSerializer.Serialize<T>(t);
            byte[] protocol = new byte[3] {(byte)'V', (byte)'L', (byte)'L'};
            byte[] size = new byte[1] {(byte) b_type.Length};
            byte[] dist = new byte[3 + 1 + b_type.Length + b_value.Length];
            int len = 0 ;
            Buffer.BlockCopy(protocol, 0, dist,len , protocol.Length);
            Buffer.BlockCopy(size, 0, dist, len += protocol.Length , size.Length);
            Buffer.BlockCopy(b_type, 0, dist, len += size.Length, b_type.Length);
            Buffer.BlockCopy(b_value,0, dist, len += b_type.Length, b_value.Length);
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
            byte[] b_type = new byte[size];
            byte[] b_value = new byte[receiveBytes.Length - size - 4];
            Buffer.BlockCopy(receiveBytes, 4, b_type, 0, b_type.Length);
            Buffer.BlockCopy(receiveBytes, b_type.Length + 4, b_value, 0, b_value.Length);
            MessageType messageType = MessagePackSerializer.Deserialize<MessageType>(b_type);
            type = messageType;
            return b_value;
        }
    }
}