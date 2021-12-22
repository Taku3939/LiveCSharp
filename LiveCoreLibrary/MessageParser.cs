using System;
using System.Text;
using LiveCoreLibrary.Commands;
using MessagePack;

namespace LiveCoreLibrary
{
    public static class MessageParser
    {

        /// <summary>
        /// 文字列をBase64にエンコードし、byte列に変換する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns></returns>
        public static byte[] ToBase64Bytes(string str)
        {
            var secret = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            var secretBuf = System.Text.Encoding.UTF8.GetBytes(secret);

            return secretBuf;
        }

        /// <summary>
        /// 文字列をBase64にエンコードし、byte列に変換する
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToBase64Bytes(byte[] data) 
        {
            var str = Convert.ToBase64String(data);
            var encodeBuf = System.Text.Encoding.UTF8.GetBytes(str);

            return encodeBuf;
        }

        /// <summary>
        /// byte列をBase64でデコードした文字列に変換する
        /// </summary>
        /// <param name="data">データ配列</param>
        /// <returns></returns>
        private static string FromBase64BytesToString(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            var tmp = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(tmp);
        }
        /// <summary>
        /// byte列をBase64でデコードした文字列に変換する
        /// </summary>
        /// <param name="data">データ配列</param>
        /// <returns></returns>
        public static byte[] FromBase64BytesToBytes(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            var tmp = Convert.FromBase64String(str);
            
            return tmp;
        }

        public static byte[] Encode(ICommand cmd)
        {
            int terminateIdx = 1;
            var serialize = MessagePackSerializer.Serialize(cmd);
            var data = ToBase64Bytes(serialize);
            byte[] dist = new byte[data.Length + terminateIdx];
            // Headerの付加
            /* なんかデータあったらここに書こうかなと思ってる */
            // Body
            Buffer.BlockCopy(data, 0, dist, 0, data.Length);
            // Null文字の付加
            Buffer.BlockCopy(new []{(byte)'\0'}, 0, dist, data.Length, 1);
            return dist;
        }

        public static ICommand Decode(byte[] data)
        {
            int terminateIdx = 1;
            byte[] dist = new byte[data.Length - terminateIdx];
            Buffer.BlockCopy(data, 0, dist, 0, dist.Length);
            var base64dec = MessageParser.FromBase64BytesToBytes(dist);
            var cmd = MessagePackSerializer.Deserialize<ICommand>(base64dec);
            return cmd;
        }
    }
}