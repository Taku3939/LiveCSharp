using System.Net.Sockets;
using LiveCoreLibrary.Commands;

namespace LiveCoreLibrary.Messages
{
    public class ReceiveData
    {
        public ITcpCommand TcpCommand;
        public TcpClient Client;
     
        public ReceiveData(ITcpCommand tcpCommand, TcpClient client)
        {
            this.TcpCommand = tcpCommand;
            this.Client = client;
      
        }
    }
}