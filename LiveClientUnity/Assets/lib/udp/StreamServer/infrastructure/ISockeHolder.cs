using System.Net;

namespace StreamServer
{
    public interface ISocketHolder
    {
        /// <summary>
        /// UdpサーバーのIPAddressとPortの取得
        /// </summary>
        /// <returns></returns>
        IPEndPoint GetIPEndPoint();
    }
}