using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessageObject;
using UI.Hub;
using UniRx;
using VLLLiveEngine;
namespace LiveServer
{
    class Program
    {
        private static MusicValue MusicValue = new MusicValue(0);
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
                .Where(x => x.Item1.MessageTypeContext== typeof(VLLLiveEngine.Unit).ToString())
                .Where(x => x.Item1.Method == Method.Get)
                .Subscribe(async x =>
                {
                    try
                    {
                        await x.Item3.Client.SendAsync(MessageParser.Encode(MusicValue, typeof(SyncHub)), SocketFlags.None);
                        Console.WriteLine("send" + MusicValue.StartTimeCode);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                });

     
                //MusicValueの更新
            server.OnMessageReceived
                .Where(x => x.Item1.MessageTypeContext == typeof(MusicValue).ToString())
                .Where(x => x.Item1.Method == Method.Post)
                .Subscribe(async x =>
                {
                    try
                    {
                        MusicValue = MessageParser.Decode<MusicValue>(x.Item2);
                        Console.WriteLine("Set StarTime : " + MusicValue.StartTimeCode);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.ToString());
                    }
                });

            //メッセージの一括送信
            server.OnMessageReceived
                .Where(x => x.Item1.Method == Method.Post)
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

namespace UI.Hub
{
    public class SyncHub{}
}

