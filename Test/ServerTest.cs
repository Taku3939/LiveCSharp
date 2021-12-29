using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using Xunit;
using LiveServer;
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
                new ChatPacket(20, "hogehoge"),
                new ChatPacket(ulong.MaxValue, "\a\b\n\r\f\t\v\\?\'\'\",\0"),
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
            ulong id = 201244;
            PositionPacket positionPacket1 = new PositionPacket();
            PositionPacket positionPacket2 = new PositionPacket();
            PositionPacket positionPacket3 = new PositionPacket();
            PositionPacket[] positionPackets = new[] { positionPacket1, positionPacket2, positionPacket3 };
            ITcpCommand packetHolder = new PositionPacketHolder(id, positionPackets);
            var positionBuffer = MessagePackSerializer.Serialize(packetHolder);
            var positionCmd = MessagePackSerializer.Deserialize<ITcpCommand>(positionBuffer);
            commands.Add(positionCmd);


            // Chat Message
            string message = "uouo";
            ITcpCommand chatMessage = new ChatPacket(id, message);
            var messageBuffer = MessagePackSerializer.Serialize(chatMessage);
            var messageCmd = MessagePackSerializer.Deserialize<ITcpCommand>(messageBuffer);
            commands.Add(messageCmd);

            // // EndPoint
            // EndPointPacket endPointPacket1 = new EndPointPacket(Guid.NewGuid(),"127.0.0.1", 65534);
            // EndPointPacket endPointPacket2 = new EndPointPacket(Guid.NewGuid(),"169.251.0.1", 65536);
            // EndPointPacket endPointPacket3 = new EndPointPacket(Guid.NewGuid(),"0.0.0.0", 21321);
            // EndPointPacketHolder endPointPacketHolder = new EndPointPacketHolder(new[] { endPointPacket1, endPointPacket2, endPointPacket3 });
            // var endpointBuffer = MessagePackSerializer.Serialize(endPointPacketHolder);
            // var endpointCmd = MessagePackSerializer.Deserialize<EndPointPacketHolder>(endpointBuffer);
            // commands.Add(endpointCmd);

            //検証タイム
            foreach (var command in commands)
            {
                switch (command)
                {
                    case PositionPacketHolder x:
                        Assert.Equal(id, x.Id);
                        Assert.Equal(positionPackets.Length, x.PositionPackets.Length);
                        Assert.Equal(positionPacket1, x.PositionPackets[0]);
                        Assert.Equal(positionPacket2, x.PositionPackets[1]);
                        Assert.Equal(positionPacket3, x.PositionPackets[2]);
                        break;

                    case ChatPacket x:
                        Assert.Equal(id, x.Id);
                        Assert.Equal(message, x.Message);
                        break;
                    //
                    // case EndPointPacketHolder x:
                    //     Assert.Equal(x.EndPointPackets.Length, endPointPacketHolder.EndPointPackets.Length);
                    //     Assert.Equal(x.EndPointPackets[0], endPointPacketHolder.EndPointPackets[0]);
                    //     Assert.Equal(x.EndPointPackets[1], endPointPacketHolder.EndPointPackets[1]);
                    //     Assert.Equal(x.EndPointPackets[2], endPointPacketHolder.EndPointPackets[2]);
                    //     break;
                    default:
                        // キャストに失敗した場合は強制的失敗
                        _testOutputHelper.WriteLine($"Cast type error");
                        Assert.False(true);
                        break;
                }
            }
        }
        
        [Fact]
        public async Task ServerTest01()
        {
            var message = new ChatPacket(20, "hogehoge");

            #region Server Side

            //ソケット管理用クラスの作成
            var holder = new ConcurrentSocketHolder();

            //Serverの非同期ループの開始
            TcpServer tcpServer = new TcpServer(Port, holder);
            tcpServer.AcceptLoop(100);
            tcpServer.HealthCheck(100);
            tcpServer.ReceiveLoop(10);

            // //受信時のイベント登録
            // server.Subscribe(new )OnMessageReceived += async (x) =>
            // {
            //     try
            //     {
            //         var data = x.Command as ChatPacket;
            //         Assert.True(data != null);
            //         _testOutputHelper.WriteLine($"Received {x.Length.ToString()}bytes: Client -> Server");
            //
            //         Assert.Equal(message.Id, data.Id);
            //         Assert.Equal(message.Message, data.Message);
            //
            //         //全てのクライアントへの送信
            //         var sendData = MessageParser.Encode(data);
            //         foreach (var c in holder.GetClients())
            //             await c.Client.SendAsync(sendData, SocketFlags.None);
            //
            //         _testOutputHelper.WriteLine(
            //             $"Send {x.Length.ToString()}: Server -> {holder.GetClients().Count.ToString()} Clients");
            //     }
            //     catch (Exception e)
            //     {
            //         //エラー時
            //         Assert.Throws<Exception>(() => _testOutputHelper.WriteLine(e.ToString()));
            //     }
            // };

            await Task.Delay(1000);

            #endregion

            #region Client Side

            //メッセージを受信できたなかった時のためのフラグ
            bool flag = false;

            Client client = new Client();
            //クライアントの接続時のコード (クライアント側)
            client.OnConnected += () => _testOutputHelper.WriteLine("Connected : client");

            //受信時のイベントを追加する
            client.OnMessageReceived += (e) =>
            {
                _testOutputHelper.WriteLine($"Received {e.Length.ToString()}bytes : Server -> Client");
                var data = e.TcpCommand as ChatPacket;
                Assert.True(data != null);
                Assert.Equal(message.Id, data.Id);
                Assert.Equal(message.Message, data.Message);
                flag = true;
            };
            //接続要求
            await client.ConnectAsync(IPAddress.Parse(Host), Port);

            //受信用ループの開始
            client.ReceiveStart(10);

            client.SendAsync(message);
            _testOutputHelper.WriteLine($"Send : Client -> Server");


            //受信までの待機
            await Task.Delay(6000);

            //メッセージを受信できたどうかの確認
            Assert.True(flag);
            client.Close();
            tcpServer.Close();

            #endregion
        }
    }
}