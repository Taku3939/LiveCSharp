using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace LiveServer
{
    public class SocketHolder : ISocketHolder
    {
        private readonly object lockObject = new object();
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        
        public List<TcpClient> GetClients()
        {
            lock (lockObject)
            {
                return this._clients;
            }
        }

        public void AddClient(TcpClient client)
        {
            lock (lockObject)
            {
                if(!this._clients.Contains(client))
                    this._clients.Add(client);
            }
        }

        public void Remove(TcpClient client)
        {
            lock (lockObject)
            {
                this._clients.Remove(client);
            }
        }
    }
}