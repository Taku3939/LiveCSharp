using System;
using MessagePack;

namespace VLLLiveEngine
{
    public static class MessageParser
    {
        /// <summary>
        /// エンコード用
        /// 0 〜 2 VLL
        /// 3 size
        /// </summary>
        /// <param name="t"></param>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] Encode<T>(T t, Type source)
        {
            var type = new Message(Method.Post, typeof(T), source);
            var bType = MessagePackSerializer.Serialize(type);
            var bValue = MessagePackSerializer.Serialize<T>(t);
            var dist = new byte[5 + bType.Length + bValue.Length];
            unsafe
            {
                fixed (byte* p = dist)
                {
                    *(char*) &p[0] = 'V';
                    *(char*) &p[1] = 'L';
                    *(char*) &p[2] = 'L';
                    *(int*) &p[3] = bType.Length;
                    *(int*) &p[4] = bValue.Length;
                    
                    // int size = Marshal.SizeOf(dist[5]) * dist.Length;
                    // IntPtr intPtr = Marshal.AllocHGlobal(size);
                    // Marshal.Copy(dist, 0, intPtr, dist.Length);
                    //
                    // size = Marshal.SizeOf(dist[5 + bType.Length]) * dist.Length;
                    // intPtr = Marshal.AllocHGlobal(size);
                    // Marshal.Copy(dist, 0, intPtr, size);
                }
            }
            
            Buffer.BlockCopy(bType, 0, dist, 5, bType.Length);
            Buffer.BlockCopy(bValue, 0, dist, 5 + bType.Length, bValue.Length);
            return dist;
        }


        public static byte[] EncodeCustom<T>(Method method, T t, Type v)
        {
            var type = new Message(method, typeof(T), v);
            var bType = MessagePackSerializer.Serialize(type);
            var bValue = MessagePackSerializer.Serialize<T>(t);
            var dist = new byte[5 + bType.Length + bValue.Length];
            unsafe
            {
                fixed (byte* p = dist)
                {
                    *(char*) &p[0] = 'V';
                    *(char*) &p[1] = 'L';
                    *(char*) &p[2] = 'L';
                    *(int*) &p[3] = bType.Length;
                    *(int*) &p[4] = bValue.Length;
                    
                    // int size = Marshal.SizeOf(dist[5]) * dist.Length;
                    // IntPtr intPtr = Marshal.AllocHGlobal(size);
                    // Marshal.Copy(dist, 0, intPtr, dist.Length);
                    //
                    // size = Marshal.SizeOf(dist[5 + bType.Length]) * dist.Length;
                    // intPtr = Marshal.AllocHGlobal(size);
                    // Marshal.Copy(dist, 0, intPtr, size);
                }
            }
            Buffer.BlockCopy(bType, 0, dist, 5, bType.Length);
            Buffer.BlockCopy(bValue, 0, dist, 5 + bType.Length, bValue.Length);
            return dist;
        }

        public static bool CheckProtocol(byte[] bytes)
        {
            if (bytes.Length <= 5) return false;

            char V = (char) bytes[0];
            char L = (char) bytes[1];
            char L2 = (char) bytes[2];
            if (V == 'V' && L == 'L' && L2 == 'L') return true;
            return false;
        }

        /// <summary>
        /// デコード用
        /// </summary>
        /// <param name="receiveBytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static byte[] Decode(byte[] receiveBytes, out Message type)
        {
            int size = receiveBytes[3];
            int valueSize = receiveBytes[4];
            byte[] bType = new byte[size];
            byte[] bValue = new byte[valueSize];
            Buffer.BlockCopy(receiveBytes, 5, bType, 0, bType.Length);
            Buffer.BlockCopy(receiveBytes, bType.Length + 5, bValue, 0, bValue.Length);
            Message message = MessagePackSerializer.Deserialize<Message>(bType);
            type = message;
            return bValue;
        }

        /// <summary>
        /// デコード用
        /// </summary>
        /// <param name="receiveBytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T Decode<T>(byte[] receiveBytes)
        {
            int size = receiveBytes[3];
            int valueSize = receiveBytes[4];
            byte[] b_value = new byte[valueSize];
            Buffer.BlockCopy(receiveBytes, size + 5, b_value, 0, b_value.Length);
            T value = MessagePackSerializer.Deserialize<T>(b_value);
            return value;
        }

        public static Message DecodeType(byte[] receiveBytes)
        {
            int size = receiveBytes[3];
            byte[] b_type = new byte[size];
            Buffer.BlockCopy(receiveBytes, 5, b_type, 0, size);
            Message message = MessagePackSerializer.Deserialize<Message>(b_type);
            return message;
        }
    }
}