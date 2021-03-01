using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LiveCoreLibrary;
using MessagePack;
using UniRx;

namespace LiveClient
{
    public class Client
    {
        private readonly TcpClient client;
        private CancellationTokenSource Source;
        public Client() => client = new TcpClient();
        public async Task Connect(string host, int port) => await client.ConnectAsync(host, port);

        private readonly Subject<UniRx.Tuple<MessageType, byte[]>> _subject =
            new Subject<UniRx.Tuple<MessageType, byte[]>>();

        public UniRx.IObservable<UniRx.Tuple<MessageType, byte[]>> OnMessageReceived => this._subject;

        public async Task Send(byte[] serialize)
        {
            if (!client.Connected)
            {
                Console.WriteLine("ClientがCloseしています");
                return;
            }

            await client.Client.SendAsync(serialize, SocketFlags.None);
        }

        public void ReceiveStart()
        {
            Source = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        if (client.Available != 0)
                        {
                            if (Source.IsCancellationRequested)
                                return;

                            if (!client.Connected)
                            {
                                Console.WriteLine("ClientがCloseしています");
                                return;
                            }

                            NetworkStream nStream = client.GetStream();
                            if (!nStream.CanRead)
                                return;
                            await using MemoryStream mStream = new MemoryStream();
                            byte[] buffer = new byte[256];
                            do
                            {
                                int dataSize = await nStream.ReadAsync(buffer, 0, buffer.Length);
                                await mStream.WriteAsync(buffer, 0, dataSize);
                            } while (nStream.DataAvailable);

                            byte[] receiveBytes = mStream.GetBuffer();
                            var body = MessageParser.Decode(receiveBytes, out var type);
                            _subject.OnNext(new UniRx.Tuple<MessageType, byte[]>(type, body));
                        }
                    }
                }
                catch (MessagePackSerializationException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        public void ReceiveStop() => Source?.Cancel();

        public void Close()
        {
            ReceiveStop();
            client.Close();
        }
    }
}