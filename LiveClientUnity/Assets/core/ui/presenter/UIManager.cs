using LiveClient;
using LiveCoreLibrary;
using StreamServer;
using UI.Hub;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using PlayState = LiveCoreLibrary.PlayState;

namespace UI.Presenter
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private CommentScreen commentScreen;
        [SerializeField] private SettingField settingField;
        [SerializeField] private Button sendButton, syncButton;
     
        //Udp
        [SerializeField] private DataHolder _dataHolder;
        [SerializeField] private UdpSocketHolder _socketHolder;
        
        //Character
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform respawnPoint;
        
        public double time;
        public int MusicNumber;
        public PlayState State;
        
        private Client _client;
        private ChatHub _chatHub;
        private SyncHub _syncHub;
        private ConnectionHub _connectionHub;
        private GameObject go;
        public void Start()
        {
            _client = new Client();
            _chatHub = new ChatHub(_client, commentScreen);
            _syncHub = new SyncHub(_client);
            _connectionHub = new ConnectionHub(_socketHolder, _dataHolder, _client);
            _connectionHub.Connect(_dataHolder.selfId, settingField);
            _client.OnConnected?.Subscribe(_ => OnConnectedEventHandler());
            sendButton.onClick.AddListener(() => _chatHub.Send(_dataHolder.selfId));
            syncButton.onClick.AddListener(() => _syncHub.Send(new MusicValue(time, MusicNumber, (int) State)));
        }

        private void Update()
        {
            if (!this._client.IsConnected)
            {
                //再接続
                _connectionHub.Connect(_dataHolder.selfId, settingField);
            }
        }

        private void OnConnectedEventHandler()
        {
            if (go == null) UnityEngine.Object.Destroy(go);
            this.go = UnityEngine.Object.Instantiate(prefab, respawnPoint.position, Quaternion.identity);
            go.GetComponent<ChangeCharacterData>().ChangeIcon(_dataHolder.selfId);
        }
    }

    public interface ISendable
    {
        
    }
}