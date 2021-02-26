using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

namespace LiveServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 30000;
            Server server = new Server(port);
            await server.Loop();
        }
    }

    public class Server
    {
        private static TcpListener _listener;

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine($"Listening start...:{port}");
        }

        public async Task Loop()
        {
            var client = await _listener.AcceptTcpClientAsync();
            await using NetworkStream nStream = client.GetStream();
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
        }
    }
}