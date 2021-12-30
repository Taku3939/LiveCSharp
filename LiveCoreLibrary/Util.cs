#nullable enable
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace LiveServer
{
    public static class Util
    {
        /// <summary>
        /// 接続確認用関数
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
    public static class ConcurrentDictionaryEx
    {
        public static List<TValue> Others<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            List<TValue> results = new List<TValue>();
            foreach (var keyValuePair in dictionary)
            {
                TKey t = keyValuePair.Key;
                // struct is boxing
                if (!t.Equals(key)) { results.Add(keyValuePair.Value); } 
            }

            return results;
        }

        public static TValue? Get<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
            where TValue :  class
        {
            if (dictionary.TryGetValue(key, out var value))  return value;
            return null;
        }
    }
}