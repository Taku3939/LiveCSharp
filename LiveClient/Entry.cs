﻿using System;
using System.Reflection.Emit;
using System.Threading.Tasks;
using LiveCoreLibrary.Commands;

namespace PositionClient
{
    class Entry
    {
        static string tcpHost = "127.0.0.1";
        static string udpHost = "127.0.0.1";
        static int tcpPort = 25565;
        static int udpPort = 25501;
        static string roomName = "Test";
        static Guid userId;

        public static async Task Main(string[] args)
        {
            userId = Guid.NewGuid();
            LiveNetwork.Instance.OnMessageReceivedUdp += (command) =>
            {
                switch (command)
                {
                    case PositionPacket x:
                        Console.WriteLine("Id : " + x.Id);
                        break;
                }
            };

            LiveNetwork.Instance.OnMessageReceivedTcp += (message) =>
            {
                var command = message.TcpCommand;
                switch (command)
                {
                    case ChatPacket x:
                        Console.WriteLine(x.Message);
                        break;
                }
            };

            LiveNetwork.Instance.OnJoin += async (id) =>
            {
                Console.WriteLine("join is " + id);
                await LiveNetwork.Instance.HolePunching();
            };
            await LiveNetwork.Instance.ConnectTcp(tcpHost, tcpPort);
            LiveNetwork.Instance.Join(userId, roomName);
            LiveNetwork.Instance.ConnectUdp(udpHost, udpPort);
            
            while (true)
            {
                await Position();
                Chat("uouo");
          
                await Task.Delay(2000);
            }
        }

        public async void ReConnect()
        {
            LiveNetwork.Instance.Close();
            await LiveNetwork.Instance.ConnectTcp(tcpHost, tcpPort);
            LiveNetwork.Instance.Join(userId, roomName);
            LiveNetwork.Instance.ConnectUdp(udpHost, udpPort);
        }

        public static async Task Position()
        {
            IUdpCommand command = new PositionPacket(userId, 0, 0, 0, 0, 0, 0, 0);
            await LiveNetwork.Instance.SendClients(command);
        }

        public static void Chat(string message)
        {
            ITcpCommand chat = new ChatPacket(userId, message);
            LiveNetwork.Instance.Send(chat);
        }
    }
}