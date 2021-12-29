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
    public abstract class Room : IObserver<ReceiveData>, IObserver<EndPointPacket>
    {
        private readonly object _lockObject = new();
        public readonly Guid Id;
        public readonly string Name;
        public readonly ConcurrentDictionary<Guid, SocketData> SocketHolder;
        private SocketData[] _clients;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public Room(Guid id, string name)
        {
            this.Id = id;
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
                UpdateClient();
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
                        $"[SERVER]{IPAddress.Parse(remoteEndPoint.Address.ToString())}: {remoteEndPoint.Port.ToString()} DISCONNECT");

                //Close処理
                client.Close();

                // リストの更新
                UpdateClient();
            }
        }


        /// <summary>
        /// 非接続クライアントの削除
        /// </summary>
        public void HealthCheck()
        {
            // 接続の確認 
            List<Guid> removeList = new List<Guid>();
            foreach (var data in _clients)
                if (!Util.IsConnected(data.tcpClient.Client))
                    removeList.Add(data.Guid);

            // 削除用リストの処理
            foreach (var data in removeList)
                this.Remove(data);
        }

        /// <summary>
        /// エンドポイントの更新
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="endPointPacket"></param>
        public void UpdateUdp(Guid guid, EndPointPacket endPointPacket)
        {
            if (!SocketHolder.TryGetValue(guid, out var socketData))
            {
                Console.WriteLine("Fail");
                return;
            }

            var data = socketData.Clone(endPointPacket);
            if (!SocketHolder.TryUpdate(guid, data, socketData))
            {
                Console.WriteLine("Fail");
                return;
            }

            UpdateClient();
        }

        /// <summary>
        /// DeepCopy
        /// </summary>
        void UpdateClient()
        {
            var count = SocketHolder.Count;
            SocketData[] copyData = new SocketData[count];
            var dict = SocketHolder.ToArray();
            for (int i = 0; i < count; i++)
                copyData[i] = new SocketData(dict[i].Key, dict[i].Value.tcpClient, dict[i].Value.udpEndPoint);

            lock (_lockObject)
            {
                this._clients = copyData;
            }
        }

        /// <summary>
        /// すべてのルーム内ユーザに向けてTcp送信
        /// </summary>
        /// <param name="tcpCommand"></param>
        public async Task TcpSend(ITcpCommand tcpCommand)
        {
            try
            {
                var data = MessagePackSerializer.Serialize(tcpCommand);

                ParallelQuery<Task<int>> tasks;
                lock (_lockObject)
                {
                    tasks = _clients
                        .AsParallel()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .Select(x => x.tcpClient.Client.SendAsync(data, SocketFlags.None));
                }

                //一応まつ
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                // ignore
            }
        }


        /// <summary>
        /// Udp送信
        /// </summary>
        /// <param name="udp"></param>
        /// <param name="udpCommand"></param>
        public async Task UdpSend(UdpServer udp, IUdpCommand udpCommand)
        {
            try
            {
                var data = MessagePackSerializer.Serialize(udpCommand);
                ParallelQuery<Task<int>> tasks;
                lock (_lockObject)
                {
                    // 非同期処理を並列実行
                    tasks =
                        _clients
                            .AsParallel()
                            .WithDegreeOfParallelism(Environment.ProcessorCount)
                            .Where(x => x.udpEndPoint.Port != -1)
                            .Select(x =>
                            {
                                var endpoint = x.udpEndPoint;
                                return udp.SendAsync(data, endpoint.Address, endpoint.Port);
                            });
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        /// <summary>
        /// メッセージ受信時
        /// </summary>
        /// <param name="value"></param>
        public abstract void OnNext(ReceiveData value);

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(EndPointPacket value)
        {
            if (!SocketHolder.TryGetValue(value.Guid, out var oldData)) return;

            SocketData newData = new SocketData(value.Guid, oldData.tcpClient, value);
            if (!SocketHolder.TryUpdate(value.Guid, newData, oldData))
            {
                Console.WriteLine("失敗");
            }
       
            // Update
            UpdateClient();
        }
    }
}