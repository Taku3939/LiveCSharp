using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;
using UniRx;

namespace LiveClient
{
    public class Client
    {
        private readonly TcpClient client;
        private CancellationTokenSource Source;
        public Client() => client = new TcpClient();

        public async Task ConnectAsync(string host, int port)
        {
            await client.ConnectAsync(host, port);
            client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
            Source = new CancellationTokenSource();
            OnConnectedSubject.OnNext(new UniRx.Unit());
        }

        private readonly Subject<UniRx.Tuple<MessageType, byte[]>> onMessageReceivedSubject =
            new Subject<UniRx.Tuple<MessageType, byte[]>>();
        private readonly Subject<UniRx.Unit> OnConnectedSubject = new Subject<UniRx.Unit>();

        public UniRx.IObservable<UniRx.Tuple<MessageType, byte[]>> OnMessageReceived => this.onMessageReceivedSubject;
        public UniRx.IObservable<UniRx.Unit> OnConnected => this.OnConnectedSubject;
        /// <summary>
        /// 非同期送信
        /// </summary>
        /// <param name="t">MessagePack Object</param>
        public void SendAsync<T>(T t)
        {
            var serialize = MessageParser.Encode(t);
            if (!client.Connected) { return; }

            var sArgs = new SocketAsyncEventArgs();
            sArgs.SetBuffer(serialize, 0, serialize.Length);
            sArgs.UserToken = serialize;
            client.Client.SendAsync(sArgs);
        }

        /// <summary>
        /// 非同期送信
        /// </summary>
        /// <param name="serialize">This byte array must be serialized by message pack</param>
        public void SendAsync(byte[] serialize)
        {
            if (!client.Connected){return;}

            var sArgs = new SocketAsyncEventArgs();
            sArgs.SetBuffer(serialize, 0, serialize.Length);
            sArgs.UserToken = serialize;
            client.Client.SendAsync(sArgs);
        }

        public void ReceiveStart()
        {
            Source = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (client.Available != 0)
                        {
                            if (Source.IsCancellationRequested)
                                return;

                            if (!client.Connected)
                            {
                                Console.WriteLine("ClientがCloseしています");
                                return;
                            }
                            NetworkStream nStream = client.GetStream();
                            if (!nStream.CanRead)
                                return;
                            await using MemoryStream mStream = new MemoryStream();
                            byte[] buffer = new byte[256];
                            do
                            {
                                int dataSize = await nStream.ReadAsync(buffer, 0, buffer.Length);
                                await mStream.WriteAsync(buffer, 0, dataSize);
                            } while (nStream.DataAvailable);

                            byte[] receiveBytes = mStream.GetBuffer();
                            if (MessageParser.CheckProtocol(buffer))
                            {
                                var body = MessageParser.Decode(receiveBytes, out var type);
                                onMessageReceivedSubject.OnNext(new UniRx.Tuple<MessageType, byte[]>(type, body));
                            }
                        }
                    }
                }
                catch (MessagePackSerializationException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            },Source.Token);
        }
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

                        await Task.Delay(interval);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, Source.Token);
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

        public void ReceiveStop() => Source?.Cancel();
        
        public void Close()
        {
            ReceiveStop();
            client.Close();
        }
    }
}