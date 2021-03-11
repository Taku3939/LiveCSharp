using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UI.infrastracture;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Presenter
{
    /// <summary>
    /// This field define the tcp and udp settings on unity runtime ui
    /// </summary>
    [Serializable]
    public class SettingField : ISetting
    {
        [SerializeField] private InputField tcpIpField, tcpPortField, udpIpField, udpPortField;
        public IPAddress GetTcpIp() => IPAddress.Parse(tcpIpField.text);

        public IPAddress GetUdpIp() => IPAddress.Parse(udpIpField.text);

        public int GetTcpPort() => int.Parse(tcpPortField.text);

        public int GetUdpPort() => int.Parse(udpPortField.text);
    }
}