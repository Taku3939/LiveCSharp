using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Utility;

namespace LiveServer
{
    /// <summary>
    /// Serverクラス
    /// </summary>
    public class TcpServer : IObservable<ReceiveData>
    {
        private readonly TcpListener _listener;
        private readonly ISocketHolder _holder;
        private readonly List<CancellationTokenSource> _sources;
        private readonly List<IObserver<ReceiveData>> _observers = new();
        private readonly ConcurrentQueue<ReceiveData> _messageQueue = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="port">ポート</param>
        /// <param name="socketHolder">TcpClientを保持するISocketHolderを実装したクラス</param>
        public TcpServer(int port, ISocketHolder socketHolder)
        {
            _holder = socketHolder;
            _sources = new List<CancellationTokenSource>();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
#if DEBUG
            Console.WriteLine($"[SERVER]Listening start... : {port}");
#endif
        }

        /// <summary>
        /// 接続状態監視用ループ
        /// 切断時リストからクライアントを削除します
        /// </summary>
        /// <param name="interval"></param>
        public void HealthCheck(int interval)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            _sources.Add(source);
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (source.IsCancellationRequested) return;
                        var clients = _holder.GetClients();
                        List<TcpClient> removeList = new List<TcpClient>();

                        // 削除用リストに追加
                        foreach (var client in clients)
                            if (!client.Connected)
                                removeList.Add(client);


                        foreach (var client in removeList)
                        {
                            if (client.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
                                Console.WriteLine(
                                    $"[SERVER]{IPAddress.Parse(remoteEndPoint.Address.ToString())}: {remoteEndPoint.Port.ToString()} DISCONNECT");

                            foreach (var o in _observers)
                            {

                                var disconnectUser = new Disconnect();
                                o.OnNext(new ReceiveData(disconnectUser, client));

                            }

                            // Close and Remove
                            _holder.Remove(client);
                        }

                        await Task.Delay(interval, source.Token);
                        source.Token.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Task Canceled");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, source.Token);
        }

        /// <summary>
        /// 接続確認用関数
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// 接続用ループ
        /// </summary>
        /// <param name="interval"></param>
        public void AcceptLoop(int interval)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            _sources.Add(source);
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (source.IsCancellationRequested) return;
                        List<Task<TcpClient>> tasks = new List<Task<TcpClient>>();
                        while (_listener.Pending())
                        {
                            var task = _listener.AcceptTcpClientAsync();
                            tasks.Add(task);
                        }

                        await Task.WhenAll(tasks);

                        foreach (var task in tasks)
                        {
                            var client = task.Result;
                            var remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                            _holder.Add(client);
#if DEBUG
                            if (remoteEndPoint != null)
                                Console.WriteLine(
                                    $"[SERVER]{remoteEndPoint.Address}: {remoteEndPoint.Port.ToString()} CONNECT");
                            Console.WriteLine($"[SERVER] client count : {_holder.GetClients().Count.ToString()}");
#endif
                        }

                        await Task.Delay(interval, source.Token);
                        source.Token.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Task Canceled");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, source.Token);
        }

        /// <summary>
        /// 受信用ループ
        /// 受信したデータをキューにぶち込みます
        /// </summary>
        /// <param name="interval"></param>
        public void ReceiveLoop(int interval)
        {
            //受信関数キャンセル用のトークンの作成
            CancellationTokenSource source = new CancellationTokenSource();
            _sources.Add(source);
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // 受信用関数の停止
                        if (source.IsCancellationRequested) return;

                        //　クライアントの取得
                        var clients = _holder.GetClients();

                        // エラー時のクライアント削除用リスト
                        Queue<TcpClient> rmCl = new Queue<TcpClient>();

                        List<Task> receiveEvents = new List<Task>();
                        //　クライアントごとに処理を行う
                        foreach (var client in clients)
                        {
                            try
                            {
                                // クライアントがCloseしていた場合
                                if (!client.Connected)
                                {
                                    if (rmCl.Contains(client)) rmCl.Enqueue(client);
                                    continue;
                                }

                                // ネットワークストリームの取得
                                NetworkStream nStream = client.GetStream();

                                if (!nStream.CanRead)
                                {
                                    Console.WriteLine(
                                        $"WARNING >> {client.Client.RemoteEndPoint} : Can not read this NetworkStream");
                                    continue;
                                }

                                // メモリストリーム上に受信したデータの書き込み
                                // ※ 受信データに制限を設けていない
                                await using MemoryStream mStream = new MemoryStream();

                                while (nStream.DataAvailable)
                                {
                                    byte[] buffer = new byte[256];
                                    int i;
                                    while ((i = await nStream.ReadAsync(buffer, 0, buffer.Length, source.Token)) > 0)
                                    {
                                        // ストリームに書き込み
                                        await mStream.WriteAsync(buffer, 0, i, source.Token);

                                        // Null文字を受信時終了
                                        if ((char)buffer[i - 1] == '\0') break;
                                    }

                                    // Null文字を除いた受信データの取り出し
                                    byte[] dist = mStream.ToArray();
                                    receiveEvents.Add(Task.Run(() =>
                                    {
                                        //　イベント関数のコール
                                        if (client.Connected)
                                            //if (client.Connected && MessageParser.CheckProtocol(dist))
                                        {
                                            var command = MessageParser.Decode(dist);
                                            _messageQueue.Enqueue(new(command, client));
                                        }
                                    }, source.Token));
                                }

                                //受信時のイベントを実行
                                // ※ とりあえずawaitしているが消すべきかな？
                                await Task.WhenAll(receiveEvents);
                                await Task.Delay(interval, source.Token);
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine(e);
                            }
                            catch (SocketException e)
                            {
                                // SocketException時にのみ強制的に切断用リストに入れる
                                if (rmCl.Contains(client)) rmCl.Enqueue(client);
                                Console.WriteLine(e.ToString());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("ReceiveLoop : Task Canceled");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }, source.Token);
        }


        public void Process(int interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        while (!_messageQueue.IsEmpty)
                        {
                            if (!_messageQueue.TryDequeue(out var data))
                                continue;

                            // 受信データの並列実行
                            _observers
                                .AsParallel()
                                .WithDegreeOfParallelism(Environment.ProcessorCount)
                                .ForAll(x => x.OnNext(data));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    await Task.Delay(interval);
                }
            });
        }

        /// <summary>
        /// 終了
        /// </summary>
        public void Close()
        {
            Console.WriteLine("hogehgeohgoehgoehg");
            //全てのループの終了
            foreach (var cancellationTokenSource in _sources)
                cancellationTokenSource.Cancel();

            //クライアントごとのclose
            var clients = _holder.GetClients();
            foreach (var c in clients) c.Close();
        }

        /// <summary>
        /// オブザーバーのコレクションを保持します。
        /// </summary>
        public IDisposable Subscribe(IObserver<ReceiveData> observer)
        {
            _observers.Add(observer);
            var dispose = new NotifyDispose<ReceiveData>(_observers, observer);
            return dispose;
        }
    }
}