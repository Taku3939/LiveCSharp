using System.Net.Sockets;
using LiveCoreLibrary.Commands;

namespace LiveCoreLibrary
{
    public class ReceiveData
    {
        public ITcpCommand TcpCommand;
        public TcpClient Client;
        public int Length;

        public ReceiveData(ITcpCommand tcpCommand, TcpClient client, int length)
        {
            this.TcpCommand = tcpCommand;
            this.Client = client;
            this.Length = length;
        }
    }
}