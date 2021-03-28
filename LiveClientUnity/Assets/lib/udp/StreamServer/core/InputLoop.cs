using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using StreamServer.Model;
using UnityEngine;

namespace StreamServer
{
    /**
     * UDP packet receiving class.
     * Do not make multiple instance of this class,
     * because that will break synchronization between
     * Socket.Available() and Socket.ReadAsync().
     * This class will be refactored to singleton.
     */
    public class InputLoop
    {
        public readonly CancellationTokenSource cts;
        private readonly UdpClient udp;
        private readonly int interval;
        private readonly string name;
        private readonly IDataHolder _dataHolder;
        private readonly IPlayerSpawner _playerSpawner;

        public InputLoop(UdpClient udpClient, IDataHolder dataHolder, IPlayerSpawner playerSpawner, int interval, string name = "Receiver")
        {
            cts = new CancellationTokenSource();
            udp = udpClient;
            this._dataHolder = dataHolder;
            _playerSpawner = playerSpawner;
            this.interval = interval;
            this.name = name;
        }
        
        public void Start()
        {
            IPEndPoint localEndPoint = udp.Client.LocalEndPoint as IPEndPoint;
            PrintDbg($"Any -> localhost: [{localEndPoint?.Port.ToString()}]\n");
            Task.Run(() => Loop(cts.Token), cts.Token);
        }

        private async Task Loop(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    try
                    {
                        var users = _dataHolder.GetDict();
                        while (udp.Available > 0)
                        {
                            var res = await udp.ReceiveAsync();
                            var buf = res.Buffer;
                            var packets = Utility.BufferToPackets(buf);
                            //Add user
                            if (packets != null)
                            {
                                foreach (var packet in packets)
                                {
                                    var user = users.ContainsKey(packet.PaketId) && users[packet.PaketId].IsConnected
                                        ? users[packet.PaketId] = new User(users[packet.PaketId])
                                        : users[packet.PaketId] = new User(packet.PaketId);
                                    if (!user.IsConnected)
                                    {
                                        Utility.PrintDbg($"Connected: [{user.UserId.ToString()}] " +
                                                         $"({res.RemoteEndPoint.Address}: {res.RemoteEndPoint.Port.ToString()})");
                                        if(packet.PaketId != _dataHolder.GetSelfId())
                                            await _playerSpawner.Spawn(packet);
                                    }
                                    user.CurrentPacket = packet;
                                    user.DateTimeBox = new DateTimeBox(DateTime.Now);
                                    user.IsConnected = true;
                                    users[packet.PaketId] = user;
                                }
                            }
                        }
                        //Delete user
                        foreach (var kvp in users)
                        {
                            var user = kvp.Value;
                            var packet = user.CurrentPacket;
                            if (user.IsConnected && packet != null && DateTime.Now - user.DateTimeBox.LastUpdated > new TimeSpan(0, 0, 1))
                            {
                                Utility.PrintDbg($"Disconnected: [{user.UserId.ToString()}] ");
                                //user.CurrentPacket = packet = null;
                                user.IsConnected = false;
                                _dataHolder.GetDict().Remove(kvp.Key);
                                // if(packet.PaketId != _dataHolder.selfId)
                                //     _playerSpawner.Remove(user.UserId);
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        PrintDbg(e.ToString());
                        PrintDbg("Is the server running？");
                        await Task.Delay(1000, token);
                    }
                    await Task.Delay(interval, token);
                }
            }
            catch (OperationCanceledException)
            {
                PrintDbg("Input loop stopped");
                udp.Close();
            }
            // catch (Exception e)
            // {
            //     Debug.LogError(e);
            //     throw;
            // }
        }
        
        public void Stop()
        {
            cts.Cancel();
        }
        
        private void PrintDbg(string str)
        {
            Debug.Log($"[{name}] {str}");
        }
    }
}
