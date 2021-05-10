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
        public bool IsDisposed => this.client == null;
        public bool IsConnected => this.client.Connected && this.client != null;
        private TcpClient client;
        private CancellationTokenSource Source;
        private readonly SynchronizationContext _context;

        private readonly Subject<UniRx.Tuple<MessageType, byte[]>> onMessageReceivedSubject =
            new Subject<UniRx.Tuple<MessageType, byte[]>>();

        private readonly Subject<UniRx.Unit> OnConnectedSubject = new Subject<UniRx.Unit>();
        private readonly Subject<UniRx.Unit> OnDisconnectedSubject = new Subject<UniRx.Unit>();
        public UniRx.IObservable<UniRx.Tuple<MessageType, byte[]>> OnMessageReceived => this.onMessageReceivedSubject;
        public UniRx.IObservable<UniRx.Unit> OnConnected => this.OnConnectedSubject;
        public UniRx.IObservable<UniRx.Unit> OnDisconnected => this.OnDisconnectedSubject;

        /// <summary>
        /// This Constructor must call by main thread
        /// </summary>
        public Client()
        {
            _context = SynchronizationContext.Current;
            client = new TcpClient();
        }

        /// <summary>
        /// 非同期接続
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string host, int port)
        {
            if (client == null || IsConnected)
            {
                Close();
                await Task.Delay(100);
                client = new TcpClient();
            }
            await client.ConnectAsync(host, port);
            Source = new CancellationTokenSource();
            OnConnectedSubject.OnNext(new UniRx.Unit());
        }

        /// <summary>
        /// 非同期接続
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(IPAddress host, int port)
        {
            if (client == null || IsConnected)
            {
                Close();
                await Task.Delay(100);
                client = new TcpClient();
            }
            await client.ConnectAsync(host, port);
            Source = new CancellationTokenSource();
            OnConnectedSubject.OnNext(new UniRx.Unit());
        }

        /// <summary>
        /// 非同期送信
        /// </summary>
        /// <param name="t">MessagePack Object</param>
        public void SendAsync<T>(T t)
        {
            var serialize = MessageParser.Encode(t);
            if (!client.Connected)
            {
                Console.WriteLine("ClientがCloseしています");
                return;
            }

            var sArgs = new SocketAsyncEventArgs();
            sArgs.SetBuffer(serialize, 0, serialize.Length);
            sArgs.UserToken = serialize;
            client.Client.SendAsync(sArgs);
        }

        /// <summary>
        /// 非同期送信
        /// </summary>
        /// <param name="serialize">byte array</param>
        public void SendAsync(byte[] serialize)
        {
            if (!client.Connected)
            {
                Console.WriteLine("ClientがCloseしています");
                return;
            }
            
            var sArgs = new SocketAsyncEventArgs();
            sArgs.SetBuffer(serialize, 0, serialize.Length);
            sArgs.UserToken = serialize;
            client.Client.SendAsync(sArgs);
        }

        /// <summary>
        /// 受信開始
        /// </summary>
        public void ReceiveStart(int interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Source.IsCancellationRequested) return;
                        if (client == null || !client.Connected)
                        {
                            return;
                        }

                        if (client.Available != 0)
                        {
                            NetworkStream nStream = client.GetStream();

                            using MemoryStream mStream = new MemoryStream();
                            byte[] buffer = new byte[256];
                            try
                            {
                                int dataSize = await nStream.ReadAsync(buffer, 0, buffer.Length, Source.Token);
                                await mStream.WriteAsync(buffer, 0, dataSize, Source.Token);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("256以下のメッセージしか処理しない : " + e.ToString());
                                return;
                            }

                            byte[] receiveBytes = mStream.GetBuffer();
                            if (MessageParser.CheckProtocol(buffer))
                            {
                                if (_context == null) { return; }
                                
                                var body = MessageParser.Decode(receiveBytes, out var type);
                                _context.Post(
                                    _ => onMessageReceivedSubject.OnNext(new UniRx.Tuple<MessageType, byte[]>(type, body)),
                                    null);
                            }
                        }

                        await Task.Delay(interval, Source.Token);
                    }
                    catch (MessagePackSerializationException e)
                    {
                        Console.WriteLine(e);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }, Source.Token);
        }

        /// <summary>
        /// 受信停止
        /// </summary>
        public void ReceiveStop() => Source?.Cancel();


        /// <summary>
        /// 接続状態監視用ループ
        /// 切断時リストからクライアントを削除します
        /// </summary>
        /// <param name="interval"></param>
        public void HealthCheck(int interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Source.IsCancellationRequested) return;
                        if (!CheckConnected(client.Client))
                        {
                            Close();
                        }
                        
                        await Task.Delay(interval, Source.Token);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, Source.Token);
        }

        /// <summary>
        /// コネクションのクローズ
        /// </summary>
        public void Close()
        {
            ReceiveStop();
            client?.Close();
            OnDisconnectedSubject.OnNext(new UniRx.Unit());
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