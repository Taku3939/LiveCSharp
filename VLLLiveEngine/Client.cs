using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using UniRx;

namespace LiveCoreLibrary
{
     public class Client
    {
        public bool IsDisposed => this._client == null;
        public bool IsConnected => this._client is {Connected: true};
        private TcpClient _client;
        private CancellationTokenSource _source;
        private SynchronizationContext _context;

        private readonly Subject<UniRx.Tuple<MessageType, byte[]>> _onMessageReceivedSubject =
            new Subject<UniRx.Tuple<MessageType, byte[]>>();

        private readonly Subject<LiveCoreLibrary.Unit> onConnectedSubject = new Subject<Unit>();
        private readonly Subject<LiveCoreLibrary.Unit> onDisconnectedSubject = new Subject<Unit>();
        public UniRx.IObservable<UniRx.Tuple<MessageType, byte[]>> OnMessageReceived => this._onMessageReceivedSubject;
        public UniRx.IObservable<LiveCoreLibrary.Unit> OnConnected => this.onConnectedSubject;
        public UniRx.IObservable<LiveCoreLibrary.Unit> OnDisconnected => this.onDisconnectedSubject;


        public Client()
        {
            _client = new TcpClient();
        }

        /// <summary>
        /// 非同期接続
        /// This method must call by main thread
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string host, int port)
        {
            _context = SynchronizationContext.Current;
            if (_client == null || IsConnected)
            {
                Close();
                await Task.Delay(100);
                _client = new TcpClient();
            }


            await _client.ConnectAsync(host, port);
            _source = new CancellationTokenSource();
            onConnectedSubject.OnNext(new LiveCoreLibrary.Unit());
        }
 
        /// <summary>
        /// 非同期接続
        /// This method must call by main thread
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(IPAddress host, int port)
        {
            _context = SynchronizationContext.Current;
            if (_client == null || IsConnected)
            {
                Close();
                await Task.Delay(100);
                _client = new TcpClient();
            }

            if (_client == null) return;
            await _client.ConnectAsync(host, port);
            //client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
            _source = new CancellationTokenSource();
            onConnectedSubject.OnNext(new LiveCoreLibrary.Unit());
        }

        /// <summary>
        /// 非同期送信
        /// </summary>
        /// <param name="t">MessagePack Object</param>
        /// <param name="source"></param>
        public void SendAsync<T>(T t, Type source)
        {
            if (_source == null) return;
            if (!_client.Connected)
            {
                return;
            }

            var serialize = MessageParser.Encode(t, source);
            var sArgs = new SocketAsyncEventArgs();
            sArgs.SetBuffer(serialize, 0, serialize.Length);
            sArgs.UserToken = serialize;
            _client.Client.SendAsync(sArgs);
        }

        /// <summary>
        /// 非同期送信
        /// </summary>
        /// <param name="t">MessagePack Object</param>
        /// <param name="source"></param>
        public void GetAsync<T>(T t, Type source)
        {
            if (_source == null) return;
            if (!_client.Connected)
            {
                Console.WriteLine("ClientがCloseしています");
                return;
            }

            var serialize = MessageParser.EncodeCustom(MethodType.Get, t, source);
            var sArgs = new SocketAsyncEventArgs();
            sArgs.SetBuffer(serialize, 0, serialize.Length);
            sArgs.UserToken = serialize;
            _client.Client.SendAsync(sArgs);
        }

        /// <summary>
        /// 受信開始
        /// </summary>
        public void ReceiveStart(int interval)
        {
            if (_source == null) return;
            if (!_client.Connected)
            {
                Console.WriteLine("ClientがCloseしています");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (_client == null || !_client.Connected)
                        {
                            return;
                        }

                        if (_client.Available != 0)
                        {
                            if (_source.Token.IsCancellationRequested) return;
                            NetworkStream nStream = _client.GetStream();

                            using MemoryStream mStream = new MemoryStream();
                            byte[] buffer = new byte[256];
                            do
                            {
                                int dataSize = await nStream.ReadAsync(buffer, 0, buffer.Length, _source.Token);
                                await mStream.WriteAsync(buffer, 0, dataSize, _source.Token);
                            } while (nStream.DataAvailable);

                            byte[] receiveBytes = mStream.GetBuffer();
                            if (MessageParser.CheckProtocol(buffer))
                            {
                                if (_context == null)
                                {
                                    Console.WriteLine("context is null");
                                    return;
                                }

                                Console.WriteLine("メッセージを受信しました");
                                var body = MessageParser.Decode(receiveBytes, out var type);
                                _context.Post(
                                    _ => _onMessageReceivedSubject.OnNext(new UniRx.Tuple<MessageType, byte[]>(type, body)),
                                    null);
                            }
                        }

                        await Task.Delay(interval, _source.Token);
                    }
                }
                catch (MessagePackSerializationException e)
                {
                    Console.WriteLine(e);
                }
                catch (OperationCanceledException exception)
                {
                    Console.WriteLine("Task Canceled");
                }
                catch (Exception e)
                { 
                    Console.WriteLine(e);
                }
            }, _source.Token);
        }

        /// <summary>
        /// 受信停止
        /// </summary>
        public void ReceiveStop() => _source?.Cancel();


        /// <summary>
        /// 接続状態監視用ループ
        /// 切断時リストからクライアントを削除します
        /// </summary>
        /// <param name="interval"></param>
        public void HealthCheck(int interval)
        {
            if (_source == null) return;
            if (!_client.Connected)
            {
                return;
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (_source.IsCancellationRequested) return;
                        if (!CheckConnected(_client.Client))
                        {
                            Close();
                        }

                        await Task.Delay(interval, _source.Token);
                    }
                    catch (OperationCanceledException exception)
                    {
                        Console.WriteLine("Task Canceled");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, _source.Token);
        }

        /// <summary>
        /// コネクションのクローズ
        /// </summary>
        public void Close()
        {
            ReceiveStop();
            _client?.Close();
            onDisconnectedSubject.OnNext(new Unit());
        }


        /// <summary>
        /// 接続確認用関数
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static bool CheckConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}