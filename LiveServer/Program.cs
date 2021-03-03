using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;
using UniRx;

namespace LiveServer
{
    class Program
    {
 
        private static async Task Main(string[] args)
        {
            var holder = new ConcurrentSocketHolder();
            int port = 30000;
            Time.Start(1000);
            Server server = new Server(port, holder);
            server.AcceptLoop(100);
            server.HealthCheck(100);
            server.ReceiveLoop(10);

            //現在のミュージックの取得
            server.OnMessageReceived
                .Where(e => (MethodType)e.Item1.MethodType == MethodType.GetMusicValue)
                .Where(e => e.Item1.type == typeof(LiveCoreLibrary.Unit))
                .Subscribe(async e => await e.Item3.Client.SendAsync(MessageParser.Encode(Time.Value), SocketFlags.None));
            //MusicValueの更新
            server.OnMessageReceived
                .Where(e => e.Item1.type == typeof(MusicValue))
                .Where(e => (MethodType)e.Item1.MethodType == MethodType.Post)
                .Subscribe(async e =>
                {
                    try
                    {
                        var buffer = MessageParser.Decode(e.Item2, out var type); 
                        Time.Value = MessagePackSerializer.Deserialize<MusicValue>(buffer);
                    }
                    catch (Exception exception) { Console.WriteLine(exception.ToString());}
                });
            
            //メッセージの一括送信
            server.OnMessageReceived
                .Where(e => (MethodType)e.Item1.MethodType == MethodType.Post)
                .Subscribe(async e =>
                {
                    foreach (var client in holder.GetClients())
                        await client.Client.SendAsync(e.Item2, SocketFlags.None);
                });
            
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "quit")
                {
                    //Time.Stop();
                    server.Close();
                    return;
                }
            }
        }
    }
}