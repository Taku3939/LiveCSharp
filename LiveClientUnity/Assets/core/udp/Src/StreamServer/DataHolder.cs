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
        public ConcurrentDictionary<ulong, User> Users = new ConcurrentDictionary<ulong, User>();

        public ulong selfId ;
        public string screenName;
        public void Initialize()
        {
            Users = new ConcurrentDictionary<ulong, User>();
        }
    }
}
