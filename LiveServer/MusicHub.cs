using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LiveCoreLibrary;

namespace LiveServer
{
    /// <summary>
    /// 音楽を同期させる
    /// </summary>
    public class MusicHub
    {
        private MusicValue MusicValue = new MusicValue(-1, -1);

        private ISocketHolder holder { get; }
        public MusicHub(ISocketHolder holder)
        {
            this.holder = holder;
            Task.Run(async () =>
            {
                while (true)
                {
                    if(holder.GetClients().Count == 0) continue;
                    
                    MusicValue.CurrentTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            
                    
                    if (Math.Abs(MusicValue.CurrentTime - MusicValue.StartTimeCode) < 1)
                    {
                        foreach (var client in holder.GetClients())
                            await client.Client.SendAsync(MessageParser.Encode("/m/get", MusicValue), SocketFlags.None);
                    }
                }
            });
        }

        /// <summary>
        /// リクエストに応じて時刻を送信
        /// </summary>
        /// <param name="client"></param>
        public async void GetTime(TcpClient client)
        {
            try
            {
                MusicValue.CurrentTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                await client.Client.SendAsync(MessageParser.Encode("/m/get", MusicValue), SocketFlags.None);
                Console.WriteLine("send" + MusicValue.StartTimeCode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        /// <summary>
        /// 時刻を設定
        /// </summary>
        /// <param name="value"></param>
        public async void SetTime(SetMusicValue value)
        {
            try
            {
             
                MusicValue.StartTimeCode =  new DateTime(value.year, value.month,value.day,value.hour, value.minute, value.seconds).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                MusicValue.CurrentTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        
                foreach (var client in holder.GetClients())
                    await client.Client.SendAsync(MessageParser.Encode("/m/set", MusicValue), SocketFlags.None);
            }catch(Exception){}
        }
    }
}