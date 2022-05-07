using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    public abstract class Room
    {
        protected readonly object _lockObject = new();

        public readonly string Name;
        public readonly ConcurrentDictionary<ulong, SocketData> SocketHolder;
        protected SocketData[] Clients = null;


        public event Action<ulong> OnAddEvent;
        public event Action<ulong> OnRemoveEvent;
        public event Action OnUpdateEvent;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name"></param>
        protected Room(string name)
        {
            this.Name = name;
            SocketHolder = new ConcurrentDictionary<ulong, SocketData>();
        }


        /// <summary>
        /// ルーム内にユーザの追加
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        public async void Add(ulong id, TcpClient client)
        {
            if(!SocketHolder.ContainsKey(id) && SocketHolder.TryAdd(id, new SocketData(id, client, new EndPointPacket(id, "", -1))))
            {
                Console.WriteLine($"[SERVER]:id`{id}` is join");
                UpdateClient(); 
                //wait 1s
                await Task.Delay(1000);
                OnAddEvent?.Invoke(id);
            }
        }


        /// <summary>
        /// ルームからユーザの削除
        /// </summary>
        /// <param name="id"></param>
        public async void Remove(ulong id)
        {
            if (SocketHolder.ContainsKey(id) && SocketHolder.TryRemove(id, out var socketData))
            {
                var client = socketData.tcpClient;
                if (client.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
                    Console.WriteLine(
                        $"[SERVER] : {IPAddress.Parse(remoteEndPoint.Address.ToString())}: {remoteEndPoint.Port.ToString()} id`{id.ToString()}` leave RoomName`{this.Name}`");

                // リストの更新
                UpdateClient();
                await Task.Delay(1000);
                OnRemoveEvent?.Invoke(id);
            }
        }

        /// <summary>
        /// ルームからユーザの削除
        /// </summary>
        /// <param name="guid"></param>
        public async void Remove(TcpClient client)
        {
            foreach (var keyValuePair in SocketHolder)
            {
                if (keyValuePair.Value.tcpClient == client)
                {
                    var guid = keyValuePair.Key;
                    SocketHolder.TryRemove(guid, out var data);
                    // リストの更新
                    UpdateClient();
                    await Task.Delay(1000);
                    OnRemoveEvent?.Invoke(guid);
                    break;
                }
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
            
            OnUpdateEvent?.Invoke();
        }
    }
}