using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace LiveServer
{
    public interface ISocketHolder
    {
        List<TcpClient> GetClients();
        public void Add(TcpClient client);
        public void Remove(TcpClient client);
    }
}