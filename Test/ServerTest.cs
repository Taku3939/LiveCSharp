using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using Xunit;
using LiveServer;
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
            ChatMessage[] messages = new[]
            {
                new ChatMessage(20, "hogehoge"),
                new ChatMessage(ulong.MaxValue, "\a\b\n\r\f\t\v\\?\'\'\",\0"),
            };
            foreach (var m in messages)
            {
                var enc = MessageParser.Encode(m);
                var output = MessageParser.Decode(enc) as ChatMessage;

                Assert.True(output != null, "Unable to cast encoded bytes");
                Assert.Equal(m.Id, output.Id);
                Assert.Equal(m.Message, output.Message);
            }
        }

        [Fact]
        public async Task ServerTest01()
        {
            var message = new ChatMessage(20, "hogehoge");

            #region Server Side

            //ソケット管理用クラスの作成
            var holder = new ConcurrentSocketHolder();

            //Serverの非同期ループの開始
            Server server = new Server(Port, holder);
            server.AcceptLoop(100);
            server.HealthCheck(100);
            server.ReceiveLoop(10);

            //受信時のイベント登録
            server.OnMessageReceived += async (x) =>
            {
                try
                {
                    var data = x.Command as ChatMessage;
                    Assert.True(data != null);
                    _testOutputHelper.WriteLine($"Received {x.Length.ToString()}bytes: Client -> Server");

                    Assert.Equal(message.Id, data.Id);
                    Assert.Equal(message.Message, data.Message);

                    //全てのクライアントへの送信
                    var sendData = MessageParser.Encode(data);
                    foreach (var c in holder.GetClients())
                        await c.Client.SendAsync(sendData, SocketFlags.None);

                    _testOutputHelper.WriteLine(
                        $"Send {x.Length.ToString()}: Server -> {holder.GetClients().Count.ToString()} Clients");
                }
                catch (Exception e)
                {
                    //エラー時
                    Assert.Throws<Exception>(() => _testOutputHelper.WriteLine(e.ToString()));
                }
            };

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
                var data = e.Command as ChatMessage;
                Assert.True(data != null);
                Assert.Equal(message.Id, data.Id);
                Assert.Equal(message.Message, data.Message);
                flag = true;
            };
            //接続要求
            await client.ConnectAsync(Host, Port);

            //受信用ループの開始
            client.ReceiveStart(1000);


            var serialize = MessageParser.Encode(message);
            client.SendAsync(serialize);
            _testOutputHelper.WriteLine($"Send {serialize.Length.ToString()}bytes : Client -> Server");


            //受信までの待機
            await Task.Delay(10000);

            //メッセージを受信できたどうかの確認
            Assert.True(flag);

            #endregion
        }
    }
}