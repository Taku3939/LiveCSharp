using System;
using System.Threading.Tasks;
using LiveCoreLibrary.Client;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Messages;

namespace ChatClient
{
    public class ChatClient
    {
        private readonly Tcp _tcp;
        private readonly Guid _userId;

        private string host;
        private int port;
        public ChatClient()
        {
            // クライアントインスタンスの作成
            _tcp = new Tcp();

            //イベント登録
            _tcp.OnMessageReceived += OnMessageReceived;
            _tcp.OnConnected += (r) => OnConnected();
            _tcp.OnDisconnected += OnDisconnected;
            _userId = Guid.NewGuid();
        }

        public async Task Connect(string host, int port)
        {
            this.host = host;
            this.port = port;
            while (!await _tcp.ConnectAsync(host, port)) { Console.Write("..."); }
        }


        public void Join(string roomName)
        {
            //
            Join join = new Join(_userId, roomName);
            _tcp.SendAsync(join);
        }
        public void Send(string message)
        {
            ITcpCommand m = new ChatPacket(_userId, message);
            _tcp.SendAsync(m);
        }

        private void OnMessageReceived(ReceiveData receiveData)
        {
            switch (receiveData.TcpCommand)
            {
                case ChatPacket x:
                    Console.WriteLine($"[{x.Id.ToString()}]{x.Message}");
                    break;
                default:
                    break;
            }
        }
        private void OnConnected()
        {
            //受信開始
            Console.WriteLine($"{host}:[{port.ToString()}] connect");
            _tcp.ReceiveStart(100);
        }

        private void OnDisconnected()
        {
            Console.WriteLine($"{host}:[{port.ToString()}] disconnect");
        }


        public void Close()
        {
            _tcp.Close();
        }
    }
}