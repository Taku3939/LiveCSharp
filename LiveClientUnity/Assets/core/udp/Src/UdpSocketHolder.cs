using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
 using UnityEngine.Serialization;

 [CreateAssetMenu]
public class UdpSocketHolder : ScriptableObject
{
    [SerializeField] public string serverIpAddress;
    [SerializeField] public int serverPort;
    [NonSerialized] public IPEndPoint RemoteEndPoint;

    private void OnEnable()
    {
        RemoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIpAddress), serverPort);
    }

    [NonSerialized]
    private UdpClient _udpClient;
    public UdpClient UdpClient
    {
        get
        {
            var client = _udpClient ?? (_udpClient = new UdpClient());
            try
            {
                //--Automatically bind to an available port by binding to port 0.
                client.Client.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0));
                
                var sioUdpConnectionReset = -1744830452;
                var inValue = new byte[] { 0 };
                var outValue = new byte[] { 0 };
                client.Client.IOControl(sioUdpConnectionReset, inValue, outValue);
            }
            catch (SocketException e)
            {
                //--Binding to port 0 throws SocketException.
                //Debug.Log(e);
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
}
