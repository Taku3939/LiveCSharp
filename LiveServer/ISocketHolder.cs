using System.Collections.Generic;
using System.Net.Sockets;

namespace LiveServer
{
    public interface ISocketHolder
    {
        List<TcpClient> GetClients();
        void AddClient(TcpClient client);
        void Remove(TcpClient client);
    }
}