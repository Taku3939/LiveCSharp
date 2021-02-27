using System;
using MessagePack;

namespace LiveCoreLibrary
{
    public class MessageCreator
    {
        public static byte[] Create<T>(MessageType _type, T t)
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
        public static void parse(byte[] receiveBytes)
        {
            int size = receiveBytes[0];
            byte[] b_type = new byte[size];
            byte[] b_value = new byte[receiveBytes.Length - size - 1];
            Buffer.BlockCopy(receiveBytes, 1, b_type, 0, b_type.Length);
            Buffer.BlockCopy(receiveBytes, b_type.Length + 1, b_value, 0, b_value.Length);
            var type = MessagePackSerializer.Deserialize<MessageType>(b_type);
            var value = MessagePackSerializer.Deserialize<MusicValue>(b_value);
            Console.WriteLine($"{type.type.ToString()} = {value.MusicNumber} is {value.TimeCode}");
        }
    }
}