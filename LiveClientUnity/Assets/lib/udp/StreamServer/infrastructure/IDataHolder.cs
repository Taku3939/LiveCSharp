using System.Collections.Generic;
using StreamServer.Model;

namespace StreamServer
{
    /// <summary>
    /// DataHolder interface
    /// </summary>
    public interface IDataHolder
    {
        /// <summary>
        /// 自分のid
        /// TwitterIdを用いるのでulongを使用
        /// </summary>
        /// <returns></returns>
        ulong GetSelfId();
        
        /// <summary>
        /// 全ユーザのデータを取得
        /// </summary>
        /// <returns></returns>
        IDictionary<ulong, User> GetDict();
    }
}