using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Utility;
using MessagePack;

namespace LiveServer
{
    public class UdpServer : IObservable<EndPointPacket>
    {
        private readonly UdpClient _udp;
        private readonly CancellationTokenSource _cts;
        private readonly List<IObserver<EndPointPacket>> _observers = new();
        private readonly ConcurrentQueue<EndPointPacket> _endPointPackets = new();

        public UdpServer(int port)
        {
            _cts = new CancellationTokenSource();
            // ファイアウォールが開いている特定のポートでBind
            _udp = new UdpClient(port);
            IPEndPoint localEndPoint = _udp.Client.LocalEndPoint as IPEndPoint;
            Console.WriteLine($"[SERVER] Any -> localhost: [{localEndPoint?.Port}]\n");
        }


        /// <summary>
        /// 受信用ループ
        /// </summary>
        /// <param name="interval"></param>
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
                            var endPoint = res.RemoteEndPoint;

                            //新しいクライアントがいたら増やす
                            var message = MessagePackSerializer.Deserialize<IUdpCommand>(res.Buffer);
                            if (message is HolePunchingPacket packet)
                                _endPointPackets.Enqueue(new EndPointPacket(packet.UserId,
                                    endPoint.Address.ToString(),
                                    endPoint.Port));
                        }
                    }
                    catch (MessagePackSerializationException e)
                    {
                        // ignore
                        Console.WriteLine(e);
                    }
                    catch (SocketException e)
                    {
                        if (e.ErrorCode != 10054) // Client Disconnected.
                            // Printer.PrintDbg(e, id);
                            Console.WriteLine(e.ToString());
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    await Task.Delay(interval);
                }
            }, _cts.Token);
        }

        public void Update(int interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        while (!_endPointPackets.IsEmpty)
                            if (_endPointPackets.TryDequeue(out var data))
                                foreach (var observer in _observers)
                                    observer.OnNext(data);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
         
                    await Task.Delay(interval);
                }
            }, _cts.Token);
        }

        public async Task<int> SendAsync(byte[] buf, string address, int port)
        {
            return await _udp.SendAsync(buf, buf.Length, new IPEndPoint(IPAddress.Parse(address), port));
        }

        public IDisposable Subscribe(IObserver<EndPointPacket> observer)
        {
            _observers.Add(observer);
            var dispose = new NotifyDispose<EndPointPacket>(_observers, observer);
            return dispose;
        }
    }
}