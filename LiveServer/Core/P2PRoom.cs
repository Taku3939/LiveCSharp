﻿using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Messages;
using MessagePack;

namespace LiveServer
{
    public abstract class P2PRoom : Room, IObserver<EndPointPacket>, IObserver<ReceiveData>
    {
        private readonly UdpServer _udp;

        protected P2PRoom(string name, UdpServer udp) : base(name)
        {
            this._udp = udp;
            OnAddEvent += OnAdd;
            OnRemoveEvent += OnRemove;
            OnUpdateEvent += HolePunching;
        }

        private async void OnAdd(ulong userId)
        {
            await TcpSend(new JoinResult(userId));
        }
        private async void OnRemove(ulong userId)
        {
            await TcpSend(new LeaveResult(userId));
      
        }

        async void HolePunching()
        {
            var array = SocketHolder
                .Where(x => x.Value.udpEndPoint.Port != -1)
                .Select(x => x.Value.udpEndPoint).ToArray();

            EndPointPacketHolder packetHolder = new EndPointPacketHolder(array);
            await TcpSend(packetHolder);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public abstract void OnNext(ReceiveData value);

  

        public void OnNext(EndPointPacket value)
        {
            if (!SocketHolder.TryGetValue(value.Id, out var oldData)) return;

            SocketData newData = new SocketData(value.Id, oldData.tcpClient, value);
            if (!SocketHolder.TryUpdate(value.Id, newData, oldData))
            {
                Console.WriteLine("失敗");
            }

            // Update
            UpdateClient();
        }

        /// <summary>
        /// すべてのルーム内ユーザに向けてTcp送信
        /// </summary>
        /// <param name="tcpCommand"></param>
        public async Task TcpSend(ITcpCommand tcpCommand)
        {
            try
            {
                if (Clients == null) return;

                // Null check
                var data = MessageParser.Encode(tcpCommand);
                ParallelQuery<Task<int>> tasks;
                lock (_lockObject)
                {
                    tasks = Clients
                        .AsParallel()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .Where(x => x.tcpClient is { Connected: true })
                        .Select(x => x.tcpClient.Client.SendAsync(data, SocketFlags.None));
                }

                //一応まつ
                await Task.WhenAll(tasks);
            }
            catch (SocketException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        /// <summary>
        /// Udp送信
        /// </summary>
        /// <param name="udpCommand"></param>
        public async Task UdpSend(IUdpCommand udpCommand)
        {
            try
            {
                var data = MessagePackSerializer.Serialize(udpCommand);
                ParallelQuery<Task<int>> tasks;
                lock (_lockObject)
                {
                    // 非同期処理を並列実行
                    tasks =
                        Clients
                            .AsParallel()
                            .WithDegreeOfParallelism(Environment.ProcessorCount)
                            .Where(x => x.udpEndPoint.Port != -1)
                            .Select(x =>
                            {
                                var endpoint = x.udpEndPoint;
                                return _udp.SendAsync(data, endpoint.Address, endpoint.Port);
                            });
                }

                await Task.WhenAll(tasks);

            }
            catch (SocketException e)
            {
                //ignore
                //Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}