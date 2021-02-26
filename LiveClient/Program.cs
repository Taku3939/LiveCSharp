using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;

namespace LiveClient
{
    class Program
    {
        private static TcpClient client;
        private static MusicValue musicValue = new MusicValue() {MusicNumber = 2, TimeCode = 22};
        static string host = "localhost";
        static int port = 30000;
        private static async Task Main(string[] args)
        {
            client = new TcpClient();
            await client.ConnectAsync(host, port);
            var serialize = MessagePackSerializer.Serialize(musicValue);
            await client.Client.SendAsync(serialize, SocketFlags.None);
            client.Close();
        }
    }
}