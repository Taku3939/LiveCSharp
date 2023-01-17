using System;
using System.Net;
using System.Threading.Tasks;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Messages;

namespace LiveCoreLibrary.Client
{
    public class LiveNetwork
    {
        /// <summary>
        /// そのうち書き直してね
        /// </summary>
        public bool IsConnected { get; private set; }
        private static LiveNetwork _instance;
        public static LiveNetwork Instance => _instance ??= new LiveNetwork();

        private Tcp _tcp;
        private Udp _udp;
        private EndPointPacketHolder _p2PClients;
        public event Action<ReceiveData> OnMessageReceivedTcpEvent;
        public event Action<IUdpCommand> OnMessageReceivedUdpEvent;
        public event Action OnConnectedEvent;
        public event Action<ulong> OnJoinEvent;
        public event Action<ulong> OnLeaveEvent;

        public event Action OnCloseEvent;
        private ulong _userId;

        /// <summary>
        /// Udpホールパンチングにより、Portの開放を行う
        /// Udp接続の開始時に使う必要がある
        /// </summary>
        public async Task HolePunching()
        {
            IUdpCommand endPointPacket = new HolePunchingPacket(this._userId);
            if(_udp != null) await _udp.SendServer(endPointPacket);
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
        public void Join(ulong userId, string roomName, string prefix)
        {
            this._userId = userId;
            ITcpCommand join = new Join(userId, roomName, prefix);
            _tcp.SendAsync(join);
        }

        /// <summary>
        /// サーバー側未実装
        /// </summary>
        public void Leave()
        {
            ITcpCommand leave = new Leave(_userId);
            _tcp.SendAsync(leave);
            
            // P2Pのリストを空に
            _p2PClients = null;
            _udp.Close();
            _udp = null;
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
            _tcp.OnClose += OnCloseEvent;
            
            var addresses = await Dns.GetHostAddressesAsync(host);
            var address = addresses[0];
            
            // 接続するまで待機
            while (!await _tcp.ConnectAsync(address, port)) Console.Write("...");
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
            var addresses = await Dns.GetHostAddressesAsync(host);
            var address = addresses[0];
            _udp = new Udp(_userId, new IPEndPoint(address, port));
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
                    OnJoinEvent?.Invoke(x.UserId);
                    break;
                case LeaveResult x:
                    OnLeaveEvent?.Invoke(x.UserId);
                    break;
                case EndPointPacketHolder x:
                    _p2PClients = x;
                    break;
                default:
                    OnMessageReceivedTcpEvent?.Invoke(receiveData);
                    break;
            }
            
       
        }


        private void OnMessageReceivedOfUdp(IUdpCommand command)
        {
            OnMessageReceivedUdpEvent?.Invoke(command);
        }

        private void OnConnect(IPEndPoint ipEndPoint)
        {
            //Log
            Console.WriteLine($"[CLIENT]{ipEndPoint.Address}:[{ipEndPoint.Port.ToString()}] tcp connect");

            //受信開始
            _tcp.ReceiveStart(100);
            OnConnectedEvent?.Invoke();

            IsConnected = true;
        }

        private void OnDisconnected()
        {
            if (_tcp != null && _udp != null)
            {
                //_tcp.Close(); // 一応
                _udp.Close();
                
                Console.WriteLine("一応Close");
            } 
            Console.WriteLine($"[CLIENT]disconnect");

            IsConnected = false;
        }
    }
}