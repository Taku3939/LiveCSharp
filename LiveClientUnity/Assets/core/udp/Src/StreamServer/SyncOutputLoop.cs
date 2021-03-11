using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace StreamServer
{
    public class SyncOutputLoop
    {
        private UdpClient udp;
        private long _logName;
        private UdpSocketHolder _udpSocketHolder;

        public List<Transform> TransformList = new List<Transform>();

        public SyncOutputLoop(UdpClient udpClient, UdpSocketHolder udpSocketHolder ,long logName)
        {
            udp = udpClient;
            _udpSocketHolder = udpSocketHolder;
            _logName = logName;
        }
        
        public void Start()
        {
            IPEndPoint localEndPoint = udp.Client.LocalEndPoint as IPEndPoint;
            IPEndPoint remoteEndPoint = _udpSocketHolder.RemoteEndPoint;
            PrintDbg($"[{localEndPoint?.Address}]: [{localEndPoint?.Port}] -> " +
                     $"[{remoteEndPoint?.Address}]: [{remoteEndPoint?.Port}]\n");
        }

        public void Send(byte[] buff)
        {
            udp.Send(buff, buff.Length, _udpSocketHolder.RemoteEndPoint);
        }

        private void PrintDbg(string str)
        {
            Debug.Log($"[{_logName}] {str}");
        }
    }
}
