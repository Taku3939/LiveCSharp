using System.Net;
using System.Net.Sockets;
using StreamServer;
using UnityEngine;

namespace Udp
{
    [CreateAssetMenu]
    public class UdpSocketHolder : ScriptableObject, ISocketHolder
    {
        [SerializeField] public string serverIpAddress;
        [SerializeField] public int serverPort;
        private UdpClient _udpClient;

        public UdpClient UdpClient
        {
            get
            {
                var client = _udpClient ??= new UdpClient();
                try
                {
                    //--Automatically bind to an available port by binding to port 0.
                    client.Client.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0));

                    var sioUdpConnectionReset = -1744830452;
                    var inValue = new byte[] {0};
                    var outValue = new byte[] {0};
                    client.Client.IOControl(sioUdpConnectionReset, inValue, outValue);
                }
                catch (SocketException e)
                {
                    //--Binding to port 0 throws SocketException.
                    Debug.Log(e);
                }

                return client;
            }
        }

        public void TryClose()
        {
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }
        }

        public void SetIPEndPoint(IPEndPoint ipEndPoint)
        {
            this.serverIpAddress = ipEndPoint.Address.ToString();
            this.serverPort = ipEndPoint.Port;
        }

        public IPEndPoint GetIPEndPoint() => new IPEndPoint(IPAddress.Parse(serverIpAddress), serverPort);
    }
}