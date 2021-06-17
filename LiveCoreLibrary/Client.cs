using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace LiveCoreLibrary
{
    public class Client
    {
        public bool IsDisposed => this.client == null;
        public bool IsConnected => this.client.Connected && this.client != null;
        private TcpClient client;
        private CancellationTokenSource cts;
        private readonly SynchronizationContext _context;

        public event Action<Tuple<MessageType, byte[], TcpClient>> OnMessageReceived;

        public event Action OnConnected;
        public event Action OnDisconnected;

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
            cts = new CancellationTokenSource();
            OnConnected?.Invoke();
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
            cts = new CancellationTokenSource();
            OnConnected?.Invoke();
        }

        /// <summary>
        /// 非同期送信
        /// </summary>
        /// <param name="t">MessagePack Object</param>
        public void SendAsync<T>(string rest, T t)
        {
            var serialize = MessageParser.Encode(rest, t);
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
                        if (cts.IsCancellationRequested) return;

                        while (client.Available != 0 && client.Connected)
                        {
                            NetworkStream nStream = client.GetStream();
                            if (!nStream.CanRead)
                            {
                                Console.WriteLine("Can not read");
                                return;
                            }

                            var _ = Task.Run(async () =>
                            {
                                await using MemoryStream mStream = new MemoryStream();
                                if (!mStream.CanRead) return;
                                byte[] buffer = new byte[256];
                                try
                                {
                                    int dataSize = await nStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                                    if (dataSize < 5) return;
                                    byte[] dist = new byte[dataSize];
                                    Buffer.BlockCopy(buffer, 0, dist, 0, dataSize);
                                    if (client.Connected && MessageParser.CheckProtocol(dist))
                                    {
                                        var type = MessageParser.DecodeType(dist);
                                        OnMessageReceived?.Invoke(
                                            new Tuple<MessageType, byte[], TcpClient>(type, dist, client));
                                    }
                                }
                                catch (OperationCanceledException e)
                                {
                                    Console.WriteLine("Task Canceled");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }, cts.Token);
                        }

                        await Task.Delay(interval, cts.Token);
                    }
                    catch (OperationCanceledException e)
                    {
                        Console.WriteLine("Task Canceled");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, cts.Token);
        }

        /// <summary>
        /// 受信停止
        /// </summary>
        public void ReceiveStop() => cts?.Cancel();


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
                        if (cts.IsCancellationRequested) return;
                        if (!CheckConnected(client.Client))
                        {
                            Close();
                        }

                        await Task.Delay(interval, cts.Token);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, cts.Token);
        }

        /// <summary>
        /// コネクションのクローズ
        /// </summary>
        public void Close()
        {
            ReceiveStop();
            client?.Close();
            OnDisconnected?.Invoke();
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