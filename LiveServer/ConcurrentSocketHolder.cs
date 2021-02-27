using System.Collections.Generic;
using System.Net.Sockets;

namespace LiveServer
{
    /// <summary>
    /// TcpClientをリスト管理する : thread-safe class
    /// </summary>
    public class ConcurrentSocketHolder : ISocketHolder
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

        public void Add(TcpClient client)
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