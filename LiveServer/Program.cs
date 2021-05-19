using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary;
using UniRx;

namespace LiveServer
{
    class Program
    {
        private static MusicValue MusicValue = new MusicValue(-1, -1);

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
                .Where(x => x.Item1.type == typeof(LiveCoreLibrary.Unit))
                .Where(x => x.Item1.methodType == MethodType.Get)
                .Subscribe(async x =>
                {
                    try
                    {
                        MusicValue.CurrentTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                        await x.Item3.Client.SendAsync(MessageParser.Encode(MusicValue), SocketFlags.None);
                        Console.WriteLine("send" + MusicValue.StartTimeCode);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                });


            server.OnMessageReceived
                .Where(x => x.Item1.type == typeof(SetMusicValue))
                .Subscribe(async x =>
                {
                    try
                    {
                        var value = MessageParser.Decode<SetMusicValue>(x.Item2);
                        MusicValue.StartTimeCode = new DateTime(value.year, value.month,value.day,value.hour, value.minute, value.seconds).ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                        MusicValue.CurrentTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                      
                        foreach (var client in holder.GetClients())
                            await client.Client.SendAsync(MessageParser.Encode(MusicValue), SocketFlags.None);
                    }catch(Exception){}
                });
            //MusicValueの更新
            server.OnMessageReceived
                .Where(x => x.Item1.type == typeof(MusicValue))
                .Where(x => x.Item1.methodType == MethodType.Post)
                .Subscribe(async x =>
                {
                    try
                    {
                        MusicValue = MessageParser.Decode<MusicValue>(x.Item2);
                        Console.WriteLine($"Set StarTime : { MusicValue.StartTimeCode }, Now Time : {MusicValue.CurrentTime}");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.ToString());
                    }
                });

            //メッセージの一括送信
            server.OnMessageReceived
                .Where(x => (MethodType) x.Item1.methodType == MethodType.Post)
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

            // //チャットをコンソールに表示するためのDebug用
            // server.OnMessageReceived
            //     .Where(x => x.Item1.methodType == MethodType.Post)
            //     .Where(x => x.Item1.type == typeof(ChatMessage))
            //     .Subscribe(x =>
            //     {
            //         Console.Write("Received : ");
            //         ChatMessage chatMessage = MessageParser.Decode<ChatMessage>(x.Item2);
            //         Console.WriteLine(chatMessage.message);
            //     });

            while (true)
            {
                var line = Console.ReadLine();
                if (line == "quit")
                {
                    server.Close();
                    return;
                }
            }
        }
    }
}