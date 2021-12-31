using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary.Commands;
using MessagePack;

namespace LiveCoreLibrary.Client
{
    public class Udp
    {
        readonly UdpClient _udp;
        private readonly ConcurrentQueue<byte[]> _bufferPool;

        private CancellationTokenSource _cts;
    
        private IPEndPoint _endPoint;


        public event Action<IUdpCommand> OnMessageReceived;
        private Guid UserId;

        public Udp(Guid userId, IPEndPoint endPoint)
        {
            UserId = userId;
            _cts = new CancellationTokenSource();
            _udp = new UdpClient();

            _endPoint = endPoint;
            // //適当なデータを送信
            // これいらんかも
            IUdpCommand ping = new HolePunchingPacket(userId);
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
                                OnMessageReceived?.Invoke(command);
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


        public async Task SendClients(IUdpCommand udpCommand, EndPointPacketHolder p2PClients)
        {
            try
            {
                // メッセージのシリアライズ
                var data = MessagePackSerializer.Serialize(udpCommand);
                // 送信先が存在しない場合
                if (p2PClients == null) return;

                // クライントごとに送信処理
                foreach (var udpEndPoint in p2PClients.EndPointPackets)
                {
                    // 自分のエンドポイントを取得
                    // 自分以外に送信
                    if (_udp.Client.LocalEndPoint is IPEndPoint endPoint && udpEndPoint.Port != endPoint.Port)
                    {

                        string uo = udpEndPoint.Address;
                        if (udpEndPoint.Address == "192.168.11.1") uo = "126.74.187.200";
                        await _udp.SendAsync(data, data.Length, uo, udpEndPoint.Port);
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