using System;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Messages;

namespace LiveServer.Sample
{
    public class P2PChatRoom : P2PRoom
    {
        private bool isAuth = false;
        public P2PChatRoom(string name, UdpServer udpServer) : base(name, udpServer)
        {
        }

        public override async void OnNext(ReceiveData value)
        {
            ITcpCommand tcpCommand = value.TcpCommand;
            switch (tcpCommand)
            {
                case Join x:
                    //
                    // if (isAuth)
                    // {
                    //     var prefix = x.Prefix.Split("?");
                    //     var email = prefix[0];
                    //     var password = prefix[1];
                    //
                    //     Auth auth = new Auth();
                    //     var a = auth.Authenticate(email, password);
                    //     if (a)
                    //     {
                    //         Add(x.UserId, value.Client);
                    //     }
                    //     else
                    //     {
                    //         // 認証失敗
                    //         await TcpSend(new Fault());
                    //     }
                    // }
                    
                   
                    break;
                case Leave x:
                    Remove(x.UserId);
                    break;
                
                case ChatPacket x:
                    // 受信したことのロギング
                    // Console.WriteLine($"[CLIENT]{x.Message} ({x.Id.ToString()})");
                    // Tcpで送信
                    //await TcpSend(tcpCommand);
                    break;
            }
            await TcpSend(tcpCommand);
        }
    }
}