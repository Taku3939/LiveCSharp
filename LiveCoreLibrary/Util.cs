using System.Net.Sockets;

namespace LiveServer
{
    public static class Util
    {
        /// <summary>
        /// 接続確認用関数
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}