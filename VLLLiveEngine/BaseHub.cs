using System.Threading.Tasks;
using MessagePack;
using UniRx;

namespace VLLLiveEngine
{
    /// <summary>
    /// クライアントにメッセージの送受信を行うためのベースクラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public abstract class BaseHub<T,V> 
    {
        private readonly Client client;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected BaseHub(Client client)
        {
            this.client = client;
            this.client.OnConnected.Subscribe(_ => OnConnected());
            this.client.OnDisconnected.Subscribe(_ => OnDisconnected());
            this.client.OnConnected.Subscribe(_ =>
            {
                this.client.OnMessageReceived
                    .Where(e => e.Item1.MessageTypeContext == typeof(T).ToString() && e.Item1.ReceiveTypeContext == typeof(V).ToString())
                    .Subscribe(e => this.OnReceived(MessagePackSerializer.Deserialize<T>(e.Item2)));
                Task.Delay(10);
            });
        }

        /// <summary>
        /// サーバーとの接続時のコールバック
        /// </summary>
        protected virtual void OnConnected(){}
        
        /// <summary>
        /// サーバーとの切断時のコールバック
        /// </summary>
        protected virtual void OnDisconnected(){}
        
        /// <summary>
        /// メッセージの受信時のコールバック
        /// </summary>
        /// <param name="value"></param>
        protected abstract void OnReceived(T value);
        
        /// <summary>
        /// メッセージの送信
        /// レスポンスはBaseHubのOnReceivedで発火されます
        /// <see cref="BaseHub{T,V}.OnReceived"/>
        /// </summary>
        /// <param name="value"></param>
        public void Send(T value) => client.SendAsync(value, typeof(V));
        
        /// <summary>
        /// メッセージの送信
        /// レスポンスは自クライアントのBaseHubのOnReceivedで発火されます
        /// <see cref="BaseHub{T,V}.OnReceived"/>
        /// </summary>
        /// <param name="value"></param>
        public void Get() => client.GetAsync(new Unit(), typeof(V));
        
    }
}