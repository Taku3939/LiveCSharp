using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;
using UniRx;
using UnityEngine;
using Unit = LiveCoreLibrary.Unit;

namespace LiveClient
{
    public class Client
    {
        public bool IsDisposed => this.client == null;
        public bool IsConnected => this.client != null && this.client.Connected;
        private TcpClient client;
        private CancellationTokenSource Source;
        private readonly SynchronizationContext _context;

        private readonly Subject<Tuple<MessageType, byte[]>> onMessageReceivedSubject =
            new Subject<Tuple<MessageType, byte[]>>();

        private readonly Subject<Unit> OnConnectedSubject = new Subject<Unit>();
        private readonly Subject<Unit> OnDisconnectedSubject = new Subject<Unit>();
        public IObservable<Tuple<MessageType, byte[]>> OnMessageReceived => this.onMessageReceivedSubject;
        public IObservable<Unit> OnConnected => this.OnConnectedSubject;
        public IObservable<Unit> OnDisconnected => this.OnDisconnectedSubject;

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
            OnConnectedSubject.OnNext(new LiveCoreLibrary.Unit());
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

            if (client == null) return;
            await client.ConnectAsync(host, port);
            //client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
            Source = new CancellationTokenSource();
            OnConnectedSubject.OnNext(new LiveCoreLibrary.Unit());
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
                Debug.Log("ClientがCloseしています");
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
                Debug.Log("ClientがCloseしています");
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
                            do
                            {
                                int dataSize = await nStream.ReadAsync(buffer, 0, buffer.Length, Source.Token);
                                await mStream.WriteAsync(buffer, 0, dataSize, Source.Token);
                            } while (nStream.DataAvailable);

                            byte[] receiveBytes = mStream.GetBuffer();
                            if (MessageParser.CheckProtocol(buffer))
                            {
                                if (_context == null)
                                {
                                    Debug.Log("context is null");
                                    return;
                                }

                                Debug.Log("メッセージを受信しました");
                                var body = MessageParser.Decode(receiveBytes, out var type);
                                _context.Post(
                                    _ => onMessageReceivedSubject.OnNext(new Tuple<MessageType, byte[]>(type, body)),
                                    null);
                            }
                        }

                        await Task.Delay(interval, Source.Token);
                    }
                    catch (MessagePackSerializationException e)
                    {
                        Debug.Log(e);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
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
            OnDisconnectedSubject.OnNext(new Unit());
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