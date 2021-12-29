using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary.Commands;
using MessagePack;

namespace LiveCoreLibrary
{
    public class Udp
    {
        readonly UdpClient _udp;
        private readonly ConcurrentQueue<byte[]> _bufferPool;

        private CancellationTokenSource _cts;
        private EndPointPacketHolder _p2PClients;
        private IPEndPoint _endPoint;

        private Guid UserId;

        public Udp(Guid userId, IPEndPoint endPoint)
        {
            UserId = userId;
            _cts = new CancellationTokenSource();
            _udp = new UdpClient();

            _endPoint = endPoint;
            // //適当なデータを送信
            var ping = new EndPointPacket(userId, endPoint.Address.ToString(), endPoint.Port);
            var pingBuf = MessagePackSerializer.Serialize(ping);
            _udp.Client.SendTo(pingBuf, endPoint);
            _bufferPool = new ConcurrentQueue<byte[]>();
        }

        public void ReceiveLoop(int interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (_cts.IsCancellationRequested) return;

                        while (_udp.Available > 0)
                        {
                            UdpReceiveResult res = await _udp.ReceiveAsync();
                            _bufferPool.Enqueue(res.Buffer);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return;
                    }

                    await Task.Delay(interval);
                }
            }, _cts.Token);
        }

        public void Process(int interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (_cts.IsCancellationRequested) return;
                        
                        // サーバーからのp2pリストの更新
                        while (_bufferPool.Count > 0)
                        {
                            if (_bufferPool.TryDequeue(out var buffer))
                            {
                                var command = MessagePackSerializer.Deserialize<IUdpCommand>(buffer);
                                switch (command)
                                {
                                    case EndPointPacketHolder x:
                                        _p2PClients = x;
                                        break;
                                    case EndPointPacket x:
                                        Console.WriteLine("Address : " + x.Address);
                                        break;
                                    case PositionPacket x:
                                        Console.WriteLine("Id : " + x.Id);
                                        break;
                                }
                            }
                        }

                        await Task.Delay(interval);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }, _cts.Token);
        }


        public async Task SendClients(IUdpCommand tcpCommand)
        {
            try
            {
                // メッセージのシリアライズ
                var data = MessagePackSerializer.Serialize(tcpCommand);
                // 送信先が存在しない場合
                if (_p2PClients == null) return;

                // クライントごとに送信処理
                foreach (var udpEndPoint in _p2PClients.EndPointPackets)
                {
                    // 自分のエンドポイントを取得
                    var endPoint = _udp.Client.LocalEndPoint as IPEndPoint;
                    // 自分以外に送信
                    if (udpEndPoint.Port != endPoint.Port)
                    {
                        await _udp.SendAsync(data, data.Length, udpEndPoint.Address, udpEndPoint.Port);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public IPEndPoint GetEndPoint()
        {
            return _udp.Client.LocalEndPoint as IPEndPoint;
        }
        public async Task SendServer(IUdpCommand c)
        {
            try
            {
                var data = MessagePackSerializer.Serialize(c);
                await _udp.SendAsync(data, data.Length, _endPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public void Close()
        {
            _cts.Cancel();
            _udp.Close();
        }
    }
}