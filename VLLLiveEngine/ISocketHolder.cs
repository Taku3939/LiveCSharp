using System.Collections.Generic;
using System.Net.Sockets;

namespace VLLLiveEngine
{
    public interface ISocketHolder
    {
        List<TcpClient> GetClients();
        void Add(TcpClient client);
        void Remove(TcpClient client);
    }
}