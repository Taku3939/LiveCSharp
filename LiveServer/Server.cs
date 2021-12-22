using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    /// <summary>
    /// Serverクラス
    /// </summary>
    public class Server
    {
        private readonly TcpListener _listener;
        private readonly ISocketHolder _holder;
        private readonly List<CancellationTokenSource> _sources;

        public event Action<Data> OnMessageReceived;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="port">ポート</param>
        /// <param name="socketHolder">TcpClientを保持するISocketHolderを実装したクラス</param>
        public Server(int port, ISocketHolder socketHolder)
        {
            _holder = socketHolder;
            _sources = new List<CancellationTokenSource>();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
#if DEBUG
            Console.WriteLine($"Listening start...:{port}");
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
                        foreach (var client in clients)
                            if (!IsConnected(client.Client))
                                removeList.Add(client);

                        foreach (var client in removeList)
                            _holder.Remove(client);

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
            catch (SocketException)
            {
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
                                Console.WriteLine($"Connected: [No name] " +
                                                  $"({remoteEndPoint.Address}: {remoteEndPoint.Port})");
                            Console.WriteLine("client count is " + _holder.GetClients().Count);
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
        /// 受信したデータを全てのクライアントに送信します
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
                                    break;
                                }

                                // 利用可能なデータが存在しない場合
                                //if (client.Available != 0) break;


                                // ネットワークストリームの取得
                                NetworkStream nStream = client.GetStream();

                                if (!nStream.CanRead)
                                {
                                    Console.WriteLine(
                                        $"WARNING >> {client.Client.RemoteEndPoint} : Can not read this NetworkStream");
                                    break;
                                }
                                
                                // メモリストリーム上に受信したデータの書き込み
                                // ※ 受信データに制限を設けていない
                                await using MemoryStream mStream = new MemoryStream();
                           
                                while (nStream.DataAvailable)
                                {
                                    byte[] buffer = new byte[256];
                                    int i;
                                    while ((i = nStream.Read(buffer, 0, buffer.Length)) > 0)
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
                                            OnMessageReceived?.Invoke(new (command, client, dist.Length));
                                        }
                                    }, source.Token));
                                    
                                }
                                
                                //受信時のイベントを実行
                                // ※ とりあえずawaitしているが消すべきかな？
                                await Task.WhenAll(receiveEvents);
                                await Task.Delay(interval, source.Token);
                                
                            }
                            catch(IOException){}
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
                    catch(IOException){}
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


        /// <summary>
        /// 終了
        /// </summary>
        public void Close()
        {
            //全てのループの終了
            foreach (var cancellationTokenSource in _sources)
                cancellationTokenSource.Cancel();

            //クライアントごとのclose
            var clients = _holder.GetClients();
            foreach (var c in clients)
                c.Close();
        }
    }
}