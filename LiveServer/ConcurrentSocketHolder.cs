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
        private List<TcpClient> copy = new List<TcpClient>();
        
        public List<TcpClient> GetClients()
        {
            lock (lockObject)
            {
                return copy;
            }
        }
        
        /// <summary>
        /// DeepCopy
        /// </summary>
        private void CopyList()
        {
            List<TcpClient> newList = new List<TcpClient>();
            foreach (var c in _clients)
                newList.Add(c);
            
            this.copy = newList;
        }

        
        public void Add(TcpClient client)
        {
            lock (lockObject)
            {
                if(!this._clients.Contains(client))
                    this._clients.Add(client);
                
                CopyList();
            }
        }
        
        /// <summary>
        /// Clientの削除
        /// </summary>
        /// <param name="client"></param>
        public void Remove(TcpClient client)
        {
            lock (lockObject)
            {
                this._clients.Remove(client);
                CopyList();
            }
        }
    }
}