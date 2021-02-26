using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

namespace LiveServer
{
    public class Server
    {
        private readonly TcpListener _listener;

        private readonly List<TcpClient> _clients;

        public Server(int port)
        {
            _clients = new List<TcpClient>();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine($"Listening start...:{port}");
        }

        public void AcceptLoop()
        {
            Task.Run(async () =>
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

                        if (!_clients.Contains(client))
                            _clients.Add(client);

                        Loop(client);
                        Console.WriteLine("client count is " + _clients.Count);
                    }
                }
            });
        }

        public async Task Loop(TcpClient client)
        {
            NetworkStream nStream = client.GetStream();
            if (!nStream.CanRead)
            {
                Console.WriteLine("Can not read");
                return;
            }

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
            Send(receiveBytes);

            Loop(client);
        }

        public void Send(byte[] buffer)
        {
            Task.Run(async () =>
            {
                foreach (var c in _clients)
                    if (c.Connected)
                        await c.Client.SendAsync(buffer, SocketFlags.None);
            });
        }
    }
}