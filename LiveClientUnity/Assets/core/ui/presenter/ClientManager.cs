using System.Net;
using System.Threading.Tasks;
using LiveClient;
using StreamServer;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Presenter
{
    public class ClientManager : MonoBehaviour
    {
        //IP and port setting
        [FormerlySerializedAs("iipSettingField")] [FormerlySerializedAs("settingField")] [SerializeField] private IpSettingField ipSettingField;
        //Udp Setting
        [SerializeField] private DataHolder _dataHolder;
        [SerializeField] private UdpSocketHolder _socketHolder;
        //Character
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform respawnPoint;

        [SerializeField] private Button ConnectButton;
        private Client _client;
        private GameObject go;

        public void Start()
        {
            //Connect
            _client = VLLNetwork.Client;
            ConnectButton.onClick.AddListener(async() =>
            {
                _socketHolder.RemoteEndPoint = new IPEndPoint(ipSettingField.GetUdpIp(), ipSettingField.GetUdpPort());
                _dataHolder.selfId = _dataHolder.selfId;
                await _client.ConnectAsync(ipSettingField.GetTcpIp(), ipSettingField.GetTcpPort());
            });
   
            
            //Add connected event
            _client.OnConnected.Subscribe(_ => OnConnectedEventHandler());
            _client.OnDisconnected.Subscribe(_ => OnDisconnectedEventHandler());
        }

        // private async void Update()
        // {
        //     await Task.Delay(100);
        //     if (!this._client.IsConnected)
        //     {
        //         //再接続
        //         await _client.ConnectAsync(settingField.GetTcpIp(), settingField.GetTcpPort());
        //     }
        // }

        private void OnConnectedEventHandler()
        {
            if (go == null) Destroy(go);
            this.go = Instantiate(prefab, respawnPoint.position, Quaternion.identity);
            go.GetComponent<ChangeCharacterData>().ChangeIcon(_dataHolder.selfId);
        }

        private void OnDisconnectedEventHandler()
        {
            Destroy(go);
            go = null;
        }

        private void OnApplicationQuit()
        {
            this._client?.Close();
        }
    }
}