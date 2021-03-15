using System;
using System.Collections.Concurrent;
using StreamServer.Model;
using UnityEngine;

namespace StreamServer
{
    [CreateAssetMenu]
    public class DataHolder : ScriptableObject
    {
        [NonSerialized]
        public ConcurrentDictionary<long, User> Users = new ConcurrentDictionary<long, User>();

        public long selfId ;
        public string screenName;
        public void Initialize()
        {
            Users = new ConcurrentDictionary<long, User>();
        }
    }
}
