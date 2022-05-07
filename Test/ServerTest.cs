using System;
using System.Collections.Generic;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Messages;
using Xunit;
using MessagePack;
using Xunit.Abstractions;

namespace Test
{
    public class ServerTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const string Host = "127.0.0.1";
        private const int Port = 30000;

        public ServerTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void MessageEncodeTest()
        {
            
            ChatPacket[] messages = new[]
            {
                new ChatPacket(Guid.NewGuid(), "hogehoge"),
                new ChatPacket(Guid.NewGuid(), "\a\b\n\r\f\t\v\\?\'\'\",\0"),
            };
            foreach (var m in messages)
            {
                var enc = MessageParser.Encode(m);
                var output = MessageParser.Decode(enc) as ChatPacket;

                Assert.True(output != null, "Unable to cast encoded bytes");
                Assert.Equal(m.Id, output.Id);
                Assert.Equal(m.Message, output.Message);
            }
        }


        /// <summary>
        /// メッセージが正しくキャストできるかをテストする
        /// </summary>
        [Fact]
        public void MessageCastTest()
        {
            List<ITcpCommand> commands = new List<ITcpCommand>();

            // Position Packet
            Guid id = Guid.NewGuid();

            // Chat Message
            string message = "uouo";
            ITcpCommand chatMessage = new ChatPacket(id, message);
            var messageBuffer = MessagePackSerializer.Serialize(chatMessage);
            var messageCmd = MessagePackSerializer.Deserialize<ITcpCommand>(messageBuffer);
            commands.Add(messageCmd);


            //検証タイム
            foreach (var command in commands)
            {
                switch (command)
                {
                    case ChatPacket x:
                        Assert.Equal(id, x.Id);
                        Assert.Equal(message, x.Message);
                        break;
          
                    default:
                        // キャストに失敗した場合は強制的失敗
                        _testOutputHelper.WriteLine($"Cast type error");
                        Assert.False(true);
                        break;
                }
            }
        }
        
        
        /// <summary>
        /// メッセージが正しくキャストできるかをテストする
        /// </summary>
        [Fact]
        public void UdpMessageCastTest()
        {
            List<IUdpCommand> commands = new List<IUdpCommand>();

            // Position Packet
            Guid id = Guid.NewGuid();
            Random random = new Random();
            PositionPacket posPacket = new PositionPacket(
                id,
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble()
            );
            IUdpCommand positionPacket = posPacket;
          
            var positionBuffer = MessagePackSerializer.Serialize(positionPacket);
            var positionCmd = MessagePackSerializer.Deserialize<IUdpCommand>(positionBuffer);
            commands.Add(positionCmd);

            //検証タイム
            foreach (var command in commands)
            {
                switch (command)
                {
                    case PositionPacket x:
                        Assert.Equal(posPacket.Id, x.Id);
                        Assert.Equal(posPacket.X, x.X);
                        break;
                    default:
                        // キャストに失敗した場合は強制的失敗
                        _testOutputHelper.WriteLine($"Cast type error");
                        Assert.False(true);
                        break;
                }
            }
        }

        // class Test : IObserver<ReceiveData>
        // {
        //     private readonly ITestOutputHelper _testOutputHelper;
        //     private readonly ChatPacket _chatPacket;
        //     public Test(ITestOutputHelper testOutputHelper, ChatPacket chatPacket)
        //     {
        //         this._testOutputHelper = new TestOutputHelper();
        //         this._chatPacket = chatPacket;
        //     }
        //     public void OnCompleted()
        //     {
        //     }
        //
        //     public void OnError(Exception error)
        //     {
        //
        //     }
        //
        //     public void OnNext(ReceiveData x)
        //     {
        //         try
        //         {
        //             var data = x.TcpCommand as ChatPacket;
        //             Assert.True(data != null);
        //             _testOutputHelper.WriteLine($"Received {x.Length.ToString()}bytes: Client -> Server");
        //     
        //             Assert.Equal(_chatPacket.Id, data.Id);
        //             Assert.Equal(_chatPacket.Message, data.Message);
        //     
        //             //全てのクライアントへの送信
        //             var sendData = MessageParser.Encode(data);
        //             foreach (var c in holder.GetClients())
        //                 await c.Client.SendAsync(sendData, SocketFlags.None);
        //     
        //             _testOutputHelper.WriteLine(
        //                 $"Send {x.Length.ToString()}: Server -> {holder.GetClients().Count.ToString()} Clients");
        //         }
        //         catch (Exception e)
        //         {
        //             //エラー時
        //             Assert.Throws<Exception>(() => _testOutputHelper.WriteLine(e.ToString()));
        //         }
        //     }
        // }
        //
        //
        //
        // [Fact]
        // public async Task ServerTest01()
        // {
        //     Guid guid = Guid.NewGuid();
        //     var message = new ChatPacket(guid, "hogehoge");
        //
        //     #region Server Side
        //
        //     //ソケット管理用クラスの作成
        //     var holder = new ConcurrentSocketHolder();
        //
        //     //Serverの非同期ループの開始
        //     TcpServer tcpServer = new TcpServer(Port, holder);
        //     tcpServer.AcceptLoop(100);
        //     tcpServer.HealthCheck(100);
        //     tcpServer.ReceiveLoop(10);
        //
        //     tcpServer.Subscribe(new Test());
        //     //受信時のイベント登録
        //
        //     await Task.Delay(1000);
        //
        //     #endregion
        //
        //     #region Client Side
        //
        //     //メッセージを受信できたなかった時のためのフラグ
        //     bool flag = false;
        //
        //     Tcp tcp = new Tcp();
        //     //クライアントの接続時のコード (クライアント側)
        //     tcp.OnConnected += () => _testOutputHelper.WriteLine("Connected : client");
        //
        //     //受信時のイベントを追加する
        //     tcp.OnMessageReceived += (e) =>
        //     {
        //         _testOutputHelper.WriteLine($"Received {e.Length.ToString()}bytes : Server -> Client");
        //         var data = e.TcpCommand as ChatPacket;
        //         Assert.True(data != null);
        //         Assert.Equal(message.Id, data.Id);
        //         Assert.Equal(message.Message, data.Message);
        //         flag = true;
        //     };
        //     //接続要求
        //     await tcp.ConnectAsync(IPAddress.Parse(Host), Port);
        //
        //     //受信用ループの開始
        //     tcp.ReceiveStart(10);
        //
        //     tcp.SendAsync(message);
        //     _testOutputHelper.WriteLine($"Send : Client -> Server");
        //
        //
        //     //受信までの待機
        //     await Task.Delay(6000);
        //
        //     //メッセージを受信できたどうかの確認
        //     Assert.True(flag);
        //     tcp.Close();
        //     tcpServer.Close();
        //
        //     #endregion
        // }
    }
}