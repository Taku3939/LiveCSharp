using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiveCoreLibrary;
using Xunit;
using LiveServer;
using MessagePack;
using UniRx;
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
        public async Task ServerTest01()
        {
            double TimeCode = 2000;

            #region Server Side
            
            //ソケット管理用クラスの作成
            var holder = new ConcurrentSocketHolder();
            
            //Serverの非同期ループの開始
            Server server = new Server(Port, holder);
            server.AcceptLoop(100);
            server.HealthCheck(100);
            server.ReceiveLoop(10);
            
            //受信時のイベント登録
            server.OnMessageReceived
                .Where(x => x.Item1.type == typeof(MusicValue))
                .Where(x => x.Item1.methodType == MethodType.Post)
                .Subscribe(async x =>
                {
                    try
                    {
                        _testOutputHelper.WriteLine($"Received {x.Item2.Length.ToString()}bytes: Client -> Server");
                        var value = MessageParser.Decode<MusicValue>(x.Item2);
                        Assert.Equal(value.StartTimeCode, TimeCode);

                        //全てのクライアントへの送信
                        foreach (var c in holder.GetClients())
                            await c.Client.SendAsync(x.Item2, SocketFlags.None);
                        
                        _testOutputHelper.WriteLine($"Send {x.Item2.Length.ToString()}: Server -> {holder.GetClients().Count.ToString()}Clients");
                    }
                    catch (Exception e)
                    {
                        //エラー時
                        Assert.Throws<Exception>(() => _testOutputHelper.WriteLine(e.ToString()));
                    }
                });

            await Task.Delay(1000);

            #endregion

            #region Client Side
            
            //メッセージを受信できたなかった時のためのフラグ
            bool flag = false; 
            
            Client client = new Client();
            //クライアントの接続時のコード (クライアント側)
            client.OnConnected.Subscribe(_ => _testOutputHelper.WriteLine("Connected : client"));
            
            //受信時のイベントを追加する
            client.OnMessageReceived
                .Where(e => e.Item1.type == typeof(MusicValue))
                .Subscribe(e =>
                {
                    _testOutputHelper.WriteLine($"Received {e.Item2.Length.ToString()}bytes : Server -> Client");
                    var value = MessagePackSerializer.Deserialize<MusicValue>(e.Item2);
                    Assert.Equal(value.StartTimeCode, TimeCode);
                    flag = true;
                });
            
            //接続要求
            await client.ConnectAsync(Host, Port);
            
            //受信用ループの開始
            client.ReceiveStart(1000);
            
            //メッセージの送信
            MusicValue musicValue = new MusicValue(TimeCode);
            var serialize = MessageParser.Encode(musicValue);
            client.SendAsync(serialize);
            _testOutputHelper.WriteLine($"Send {serialize.Length.ToString()}bytes : Client -> Server");
            
            //受信までの待機
            await Task.Delay(2000);
            
            //メッセージを受信できたどうかの確認
            Assert.True(flag);
            #endregion
        }
    }
}