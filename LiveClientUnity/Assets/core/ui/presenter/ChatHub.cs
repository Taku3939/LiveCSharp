using System.Linq;
using LiveClient;
using LiveCoreLibrary;
using MessagePack;
using StreamServer;
using UI.infrastracture;
using UI.Presenter;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hub
{
    /// <summary>
    /// チャット用UI
    /// </summary>
    public class ChatHub : MonoBehaviour
    {
        [SerializeField] private CommentScreen _commentScreen;
        [SerializeField] private Button SendButton;
        [SerializeField] private DataHolder dataHolder;
        private Client client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client must connect with server</param>
        /// <param name="commentScreen">UI of CommentScreen</param>
        public void Start()
        {
            this.client = VLLNetwork.Client;
            client.OnConnected?.Subscribe(_ =>
                client.OnMessageReceived?
                    .Where(e => e.Item1.type == typeof(ChatMessage))
                    .Subscribe(e => this.Received(MessagePackSerializer.Deserialize<ChatMessage>(e.Item2))));
            
            SendButton.onClick.AddListener(Send);
        }
        
        private void Send()
        {
            client.SendAsync(new ChatMessage( dataHolder.screenName, new string(
                _commentScreen
                    .GetText()
                    .Select((c, i) => new {c, i})
                    .Where(x => x.i < 32)
                    .Select(y => y.c)
                    .ToArray())));
        }

        private void Received(ChatMessage message)
        {
            Debug.Log("Received");
            _commentScreen.SetText(message);
        }
    }
}