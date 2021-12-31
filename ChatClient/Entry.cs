using System;
using System.Net;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Client;
using LiveCoreLibrary.Commands;

namespace ChatClient
{
    static class Entry
    {
        private static string host = "localhost";
        private static int port = 30000;

        private static string roomName = "Test";
        private static async Task Main(string[] args)
        {
            // クライアントインスタンスの作成
            ChatClient chatClient = new ChatClient();
            await chatClient.Connect(host, port);
            chatClient.Join(roomName);

            while (true)
            {
                var line = Console.ReadLine();
                if (line == "q") break;
                chatClient.Send(line);
                await Task.Delay(1000);
            }

            chatClient.Close();
            Console.WriteLine("終了します.");
        }
    }
}