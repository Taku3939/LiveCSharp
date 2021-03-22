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
        private static MusicValue MusicValue = new MusicValue(double.MaxValue, int.MaxValue, PlayState.Stopped);
        private static async Task Main(string[] args)
        {
            var holder = new ConcurrentSocketHolder();
            int port = 30000;
            Server server = new Server(port, holder);
            server.AcceptLoop(100);
            server.HealthCheck(100);
            server.ReceiveLoop(10);

            //現在のミュージックの取得
            server.OnMessageReceived
                .Where(x => (MethodType)x.Item1.methodType == MethodType.GetMusicValue)
                .Where(x => x.Item1.type == typeof(LiveCoreLibrary.Unit))
                .Subscribe(async x =>
                {
                    try { await x.Item3.Client.SendAsync(MessageParser.Encode(MusicValue), SocketFlags.None); }
                    catch(Exception e){Console.WriteLine(e.ToString());}
                });
            
            
            //MusicValueの更新
            server.OnMessageReceived
                .Where(x => x.Item1.type == typeof(MusicValue))
                .Where(x => x.Item1.methodType == MethodType.Post)
                .Subscribe(async x =>
                {
                    try { MusicValue = MessageParser.Decode<MusicValue>(x.Item2); }
                    catch (Exception exception) { Console.WriteLine(exception.ToString());}
                });
            
            //メッセージの一括送信
            server.OnMessageReceived
                .Where(x => (MethodType)x.Item1.methodType == MethodType.Post)
                .Subscribe(async x =>
                {
                    try
                    {
                        foreach (var client in holder.GetClients())
                            await client.Client.SendAsync(x.Item2, SocketFlags.None);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
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