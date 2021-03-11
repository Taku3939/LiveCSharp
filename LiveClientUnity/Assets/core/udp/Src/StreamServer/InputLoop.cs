﻿using System;
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
        public CancellationTokenSource cts = new CancellationTokenSource();
        private UdpClient udp;
        private int interval;
        private string name;
        private DataHolder _dataHolder;
        private readonly RemotePlayerSpawner _playerSpawner;

        public InputLoop(UdpClient udpClient, DataHolder dataHolder, RemotePlayerSpawner playerSpawner, int interval, string name = "Receiver")
        {
            udp = udpClient;
            this._dataHolder = dataHolder;
            _playerSpawner = playerSpawner;
            this.interval = interval;
            this.name = name;
        }
        
        public void Start()
        {
            IPEndPoint localEndPoint = udp.Client.LocalEndPoint as IPEndPoint;
            PrintDbg($"Any -> localhost: [{localEndPoint?.Port}]\n");
            Task.Run(() => Loop(cts.Token), cts.Token);
        }

        private async Task Loop(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    var delay = Task.Delay(interval, token);
                    try
                    {
                        var users = _dataHolder.Users;
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
                                        Utility.PrintDbg($"Connected: [{user.UserId}] " +
                                                         $"({res.RemoteEndPoint.Address}: {res.RemoteEndPoint.Port})");
                                        if(packet.PaketId != _dataHolder.selfId)
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
                                Utility.PrintDbg($"Disconnected: [{user.UserId}] ");
                                //user.CurrentPacket = packet = null;
                                user.IsConnected = false;
                                _dataHolder.Users.TryRemove(kvp.Key, out var dummy);
                                if(packet.PaketId != _dataHolder.selfId)
                                    _playerSpawner.Remove(user.UserId);
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        PrintDbg(e);
                        PrintDbg("Is the server running？");
                        try
                        {
                        }
                        catch
                        {
                            // ignored
                        }

                        await Task.Delay(1000, token);
                    }

                    token.ThrowIfCancellationRequested();
                    await delay;
                }
            }
            catch (OperationCanceledException)
            {
                PrintDbg("Input loop stopped");
                udp = null;
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        public void Stop()
        {
            cts.Cancel();
        }
        
        private void PrintDbg<T>(T str)
        {
            Debug.Log($"[{name}] {str}");
        }
    }
}
