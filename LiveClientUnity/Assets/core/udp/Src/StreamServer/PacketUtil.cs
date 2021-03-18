using System;
using System.Linq;

namespace StreamServer
{
    public static class PacketUtil
    {
        /// <summary>
        /// 2進数文字列に変換
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static string toBinary(int n)
        {
            string r = "";
            while (n != 0)
            {
                r += (n % 2 == 0 ? "0" : "1");
                n /= 2;
            }

            string reverse = "";
            for (int i = r.Length - 1; i >= 0; --i) reverse += r[i];
            return reverse;
        }

        private static int toInt(string str)
        {
            var chars = str.ToArray();
            int index = 1;
            while (str[index] == 0 && index < chars.Length) index++;
            char[] newStr = new char[chars.Length - index];

            for (int i = index; i < chars.Length; i++) newStr[i - index] = chars[i];
            return Convert.ToInt32(new string(newStr), 2);
        }


        /// <summary>
        /// float値を1byteに変換する
        /// Range : 127 ～ -127
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte ConvertByte(float value, int m)
        {
            var v = (int) (value / m);
            var h = v >= 0 ? 0 : 1; //符号 
            v = Math.Abs(v) & 1111111; //And演算
            var s = toBinary(v);
            var str = h + s.PadLeft(7, '0');
            byte buf = Convert.ToByte(str, 2);
            return buf;
        }

        /// <summary>
        /// byteをfloatに変換する
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static float ConvertFloat(byte buf, int m)
        { 
            var body = Convert.ToString(buf, 2).PadLeft(8, '0');
            var c = int.Parse(body.First().ToString());
            var c1 = (float) toInt(body);
            var value = c1 * (c == 1 ? -1 : 1) * m;
            return value;
        }
    }
}