using System.Collections.Generic;
using System.Net.Sockets;

namespace LiveServer
{
    /// <summary>
    /// TcpClientをリスト管理する : thread-safe class
    /// </summary>
    public class ConcurrentSocketHolder : ISocketHolder
    {
        private readonly object _lockObject = new object();
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private List<TcpClient> _copy = new List<TcpClient>();
        
        public List<TcpClient> GetClients()
        {
            lock (_lockObject)
            {
                return _copy;
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
            
            this._copy = newList;
        }

        
        public void Add(TcpClient client)
        {
            lock (_lockObject)
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
            lock (_lockObject)
            {
                this._clients.Remove(client);
                CopyList();
            }
        }
    }
}