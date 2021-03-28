using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using StreamServer;
using StreamServer.Model;
using UnityEngine;

namespace Udp
{
    /// <summary>
    /// ユーザデータ保持用クラス
    /// </summary>
    [CreateAssetMenu]
    public class DataHolder : ScriptableObject, IDataHolder
    {
        [NonSerialized]
        public ConcurrentDictionary<ulong, User> Users = new ConcurrentDictionary<ulong, User>();

        public ulong selfId;
        
        public string screenName;
        public void Initialize() => Users = new ConcurrentDictionary<ulong, User>();
        public ulong GetSelfId() => selfId;
        public IDictionary<ulong, User> GetDict() => Users;
    }
}
