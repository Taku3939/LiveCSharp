using System;
using System.Net.Sockets;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;

namespace LiveServer
{
    public class ChatHub 
    {
        private readonly ConcurrentSocketHolder _socketHolder;

        public ChatHub(ConcurrentSocketHolder socketHolder)
        {
            _socketHolder = socketHolder;
        }
        public async void OnMessageReceived(ChatMessage command)
        {
            Console.WriteLine($"[CLIENT]{command.Message} ({command.Id.ToString()})");
           
            foreach (var client in _socketHolder.GetClients())
            {
                await client.Client.SendAsync(MessageParser.Encode(command), SocketFlags.None);
            }
        }
    }
}