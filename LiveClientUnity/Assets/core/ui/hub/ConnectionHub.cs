using System.Collections;
using System.Collections.Generic;
using System.Net;
using LiveClient;
using LiveCoreLibrary;
using StreamServer;
using UI.infrastracture;
using UnityEngine;

namespace UI.Hub
{
    public class ConnectionHub
    {
        private readonly UdpSocketHolder _socketHolder;
        private readonly DataHolder _dataHolder;
        private readonly Client _client;

        public ConnectionHub(UdpSocketHolder socketHolder, DataHolder dataHolder, Client client)
        {
            this._socketHolder = socketHolder;
            this._dataHolder = dataHolder;
            this._client = client;
        }

        public void Connect(long id, ISetting iSetting)
        {
            _socketHolder.RemoteEndPoint = new IPEndPoint(iSetting.GetUdpIp(), iSetting.GetUdpPort());
            _dataHolder.selfId = id;
            _client.ConnectAsync(iSetting.GetTcpIp(), iSetting.GetTcpPort());
            _client.SendAsync(MessageParser.EncodeGet(MethodType.GetMusicValue));
        }
        
    }
}