using System;
using System.Net;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Xml.Resolvers;
using LiveCoreLibrary;
using LiveCoreLibrary.Client;
using LiveCoreLibrary.Commands;

namespace PositionClient
{
    public class LiveNetwork
    {
        private static LiveNetwork _instance;
        public static LiveNetwork Instance => _instance ??= new LiveNetwork();

        private Tcp _tcp;
        private Udp _udp;
        private EndPointPacketHolder _p2PClients;
        public event Action<ReceiveData> OnMessageReceivedTcp;
        public event Action<IUdpCommand> OnMessageReceivedUdp;
        public event Action OnConnected;
        public event Action<Guid> OnJoin;
        public event Action<Guid> OnLeave;
        private Guid _userId;

        /// <summary>
        /// Udpホールパンチングにより、Portの開放を行う
        /// </summary>
        public async Task HolePunching()
        {
            IUdpCommand endPointPacket = new HolePunchingPacket(this._userId);
            await _udp.SendServer(endPointPacket);
        }

        /// <summary>
        /// Udp送信
        /// </summary>
        /// <param name="udpCommand"></param>
        public async Task SendClients(IUdpCommand udpCommand)
        {
            await _udp.SendClients(udpCommand, _p2PClients);
        }

        /// <summary>
        /// Tcp送信
        /// </summary>
        /// <param name="tcpCommand"></param>
        public void Send(ITcpCommand tcpCommand)
        {
            _tcp.SendAsync(tcpCommand);
        }
        
        /// <summary>
        /// ルームに入る
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roomName"></param>
        public void Join(Guid userId, string roomName)
        {
            this._userId = userId;
            ITcpCommand join = new Join(userId, roomName);
            _tcp.SendAsync(join);
        }

        /// <summary>
        /// サーバー側未実装
        /// </summary>
        public void Leave()
        {
            ITcpCommand leave = new Leave(_userId);
            _tcp.SendAsync(leave);
        }

        /// <summary>
        /// サーバに接続する
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public async Task ConnectTcp(string host, int port)
        {
            // Tcp
            _tcp = new Tcp();

            //イベント登録
            _tcp.OnMessageReceived += OnMessageReceived;
            _tcp.OnConnected += OnConnect;
            _tcp.OnDisconnected += OnDisconnected;
            // 接続するまで待機
            while (!await _tcp.ConnectAsync(host, port)) Console.Write("...");
            Console.WriteLine(); //　改行
        }

        /// <summary>
        /// サーバーにUdp接続を行う
        /// ConnectTcpとJoinメソッドによって,ルームへ入る必要がある
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public async void ConnectUdp(string host, int port)
        {
            // Udp
            _udp = new Udp(_userId, new IPEndPoint(IPAddress.Parse(host), port));
            _udp.ReceiveLoop(10);
            _udp.Process(10);

            _udp.OnMessageReceived += OnMessageReceivedOfUdp;
            await HolePunching();
        }
        
        /// <summary>
        /// Close
        /// </summary>
        public void Close()
        {
            _udp.Close();
            _tcp.Close();
#if DEBUG
            Console.WriteLine("終了します.");
#endif
        }
        private void OnMessageReceived(ReceiveData receiveData)
        {
            switch (receiveData.TcpCommand)
            {
                case JoinResult x:
                    OnJoin?.Invoke(x.UserId);
                    break;
                case LeaveResult x:
                    OnLeave?.Invoke(x.UserId);
                    break;
                case EndPointPacketHolder x:
                    _p2PClients = x;
                    break;
                default:
                    OnMessageReceivedTcp?.Invoke(receiveData);
                    break;
            }
            
       
        }


        private void OnMessageReceivedOfUdp(IUdpCommand command)
        {
            OnMessageReceivedUdp?.Invoke(command);
        }

        private void OnConnect(IPEndPoint ipEndPoint)
        {
            //Log
            Console.WriteLine($"[CLIENT]{ipEndPoint.Address}:[{ipEndPoint.Port.ToString()}] tcp connect");

            //受信開始
            _tcp.ReceiveStart(100);
            OnConnected?.Invoke();
        }

        private void OnDisconnected()
        {
            Console.WriteLine($"[CLIENT]disconnect");
        }
    }
}