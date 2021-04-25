using System;
using MessageObject;
using VLLLiveEngine;
namespace ChatClient
{
    public class ChatHub : BaseHub<ChatMessage, ChatHub>
    {
        public ChatHub(Client client) : base(client)
        {
        }

        protected override void OnReceived(ChatMessage message)
        {
            Console.WriteLine($"Received[{message.id.ToString()}] : " + message.message);
        }
    }
}