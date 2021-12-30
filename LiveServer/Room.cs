#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using MessagePack;

namespace LiveServer
{
    public abstract class Room
    {
        protected readonly object _lockObject = new();

        public readonly string Name;
        public readonly ConcurrentDictionary<Guid, SocketData> SocketHolder;
        protected SocketData[] Clients;


        public event Action<Guid> OnJoin;
        public event Action<Guid> OnLeave;
        public event Action OnUpdate;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name"></param>
        protected Room(string name)
        {
            this.Name = name;
            SocketHolder = new ConcurrentDictionary<Guid, SocketData>();
        }


        /// <summary>
        /// ルーム内にユーザの追加
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="client"></param>
        public void Add(Guid guid, TcpClient client)
        {
            if (SocketHolder.TryAdd(guid, new SocketData(guid, client, new EndPointPacket(guid, "", -1))))
            {
                Console.WriteLine($"[SERVER]:id`{guid}` is join");
                UpdateClient(); 
                OnJoin?.Invoke(guid);
            }
        }


        /// <summary>
        /// ルームからユーザの削除
        /// </summary>
        /// <param name="guid"></param>
        public void Remove(Guid guid)
        {
            if (SocketHolder.TryRemove(guid, out var socketData))
            {
                var client = socketData.tcpClient;
                if (client.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
                    Console.WriteLine(
                        $"[SERVER]{IPAddress.Parse(remoteEndPoint.Address.ToString())}: {remoteEndPoint.Port.ToString()} id`{guid.ToString()}` leave RoomName`{this.Name}`");

                // リストの更新
                UpdateClient();

                OnLeave?.Invoke(guid);
            }
        }

        

        /// <summary>
        /// DeepCopy
        /// </summary>
        public void UpdateClient()
        {
            var count = SocketHolder.Count;
            SocketData[] copyData = new SocketData[count];
            var dict = SocketHolder.ToArray();
            for (int i = 0; i < count; i++)
                copyData[i] = new SocketData(dict[i].Key, dict[i].Value.tcpClient, dict[i].Value.udpEndPoint);
            
            lock (_lockObject)
            {
                this.Clients = copyData;
            }
            
            OnUpdate?.Invoke();
        }
    }
}