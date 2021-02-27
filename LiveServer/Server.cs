using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;

namespace LiveServer
{
    public class Server
    {
        private readonly TcpListener _listener;
        private readonly ISocketHolder _holder;
        private readonly List<CancellationTokenSource> _sources;

        public Server(int port, ISocketHolder socketHolder)
        {
            _holder = socketHolder;
            _sources = new List<CancellationTokenSource>();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine($"Listening start...:{port}");
        }

        public void HealthCheck()
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        var clients = _holder.GetClients();
                        List<TcpClient> removeList = new List<TcpClient>();
                        foreach (var client in clients)
                            if (!IsConnected(client.Client))
                                removeList.Add(client);
                        
                        foreach (var client in removeList)
                            _holder.Remove(client);
                        
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });
        }
        
        private static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public void AcceptLoop()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
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
                            var remoteEndPoint = (IPEndPoint) client.Client.RemoteEndPoint;
                            Console.WriteLine($"Connected: [No name] " +
                                              $"({remoteEndPoint.Address}: {remoteEndPoint.Port})");
                            
                            _holder.AddClient(client);
                            Console.WriteLine("client count is " + _holder.GetClients().Count);
                            //SocketLoop loop = new SocketLoop(client, _holder, 10);
                            //loop.Run();
                            //_sources.Add(loop.Cts);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });
        }

        public void ReceiveLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        foreach (var client in _holder.GetClients())
                        {
                            while (client.Available != 0)
                            {
                                NetworkStream nStream = client.GetStream();
                                if (!nStream.CanRead)
                                {
                                    Console.WriteLine("Can not read");
                                    return;
                                }

                                Task.Run(async () =>
                                {
                                    await using MemoryStream mStream = new MemoryStream();
                                    byte[] buffer = new byte[256];
                                    do
                                    {
                                        int dataSize = await nStream.ReadAsync(buffer, 0, buffer.Length);
                                        await mStream.WriteAsync(buffer, 0, dataSize);

                                    } while (nStream.DataAvailable);

                                    byte[] receiveBytes = mStream.GetBuffer();
                                    var ob = MessagePackSerializer.Deserialize<MusicValue>(receiveBytes);

                                    Console.WriteLine($"{ob.MusicNumber} is {ob.TimeCode}");

                                    foreach (var c in _holder.GetClients())
                                        if (c.Connected)
                                            await c.Client.SendAsync(buffer, SocketFlags.None);
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());

                    }
                }
            });
        }
        
        public void Close()
        {
            foreach (var cancellationTokenSource in _sources)
                cancellationTokenSource.Cancel();
        }
    }
}