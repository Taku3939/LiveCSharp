using System.Linq;
using LiveClient;
using LiveCoreLibrary;
using MessagePack;
using UI.infrastracture;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hub
{
    public class ChatHub
    {
        private readonly ICommentScreen _commentScreen;
        private readonly Client _client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client must connect with server</param>
        /// <param name="commentScreen">UI of CommentScreen</param>
        public ChatHub(Client client, ICommentScreen commentScreen)
        {
            this._client = client;
            this._commentScreen = commentScreen;
            client.OnConnected?.Subscribe(_ =>
                client.OnMessageReceived?
                    .Where(e => e.Item1.type == typeof(ChatMessage))
                    .Subscribe(e => this.Received(MessagePackSerializer.Deserialize<ChatMessage>(e.Item2))));
        }
        
        public void Send(long userId)
        {
            _client.SendAsync(new ChatMessage( userId, new string(
                _commentScreen
                    .GetText()
                    .Select((c, i) => new {c, i})
                    .Where(x => x.i < 20)
                    .Select(y => y.c)
                    .ToArray())));
        }

        private void Received(ChatMessage message) => _commentScreen.SetText(message);
    }
}