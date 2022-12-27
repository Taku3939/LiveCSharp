using LiveCoreLibrary.Client;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Messages;
using UnityEngine;

namespace LiveCoreLibrary.View
{
    public abstract class TcpView<T> : MonoBehaviour
        where T : ITcpCommand
    {
        void Awake()
        {
            LiveNetwork.Instance.OnMessageReceivedTcp += OnMessageReceivedTcp;
        }

        public void SendTcp(T command)
        {
            var tcpCommand = command as ITcpCommand;
            LiveNetwork.Instance.Send(tcpCommand);
        }

        private void OnMessageReceivedTcp(ReceiveData message)
        {
            var command = message.TcpCommand;
            switch (command)
            {
                case T x:
                    OnPacketReceived(x);
                    break;
            }
        }

        protected virtual void OnPacketReceived(T packet)
        {
        }
    }

    public abstract class UdpView<T> : MonoBehaviour
        where T : IUdpCommand
    {
        
        void Awake()
        {
            LiveNetwork.Instance.OnMessageReceivedUdp += OnMessageReceivedUdp;
        }

        public async void SendUdp(T command)
        {
            if (LiveNetwork.Instance.IsConnected)
            {
                var udpCommand = command as IUdpCommand;
                await LiveNetwork.Instance.SendClients(udpCommand);
            }
        }
        
        private void OnMessageReceivedUdp(IUdpCommand command)
        {
            switch (command)
            {
                case T x:
                    OnPacketReceived(x);
                    break;
            }
        }
        protected virtual void OnPacketReceived(T command)
        {
        }
    }
}