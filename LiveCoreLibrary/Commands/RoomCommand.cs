using System;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class Join : ITcpCommand
    {
        [Key(0)] public ulong UserId { get; }

        [Key(1)] public string RoomName { get; }
        [Key(2)] public string Prefix { get; }

        public Join(ulong userId, string roomName, string prefix)
        {
            this.UserId = userId;
            this.RoomName = roomName;
            this.Prefix = prefix;
        }
    }

    [MessagePackObject]
    public class Leave : ITcpCommand
    {
        [Key(0)] public ulong UserId { get; }

        public Leave(ulong userId)
        {
            this.UserId = userId;
        }
    }
    
    [MessagePackObject]
    public class JoinResult : ITcpCommand
    {
        [Key(0)] public ulong UserId;
        public JoinResult(ulong userId)
        {
            this.UserId = userId;
        }
    }
    
    [MessagePackObject]
    public class LeaveResult : ITcpCommand
    {
        [Key(0)] public ulong UserId;
        
        public LeaveResult(ulong userId)
        {
            this.UserId = userId;
        }
    }

    /// <summary>
    /// 送信できない
    /// </summary>
    public class Disconnect : ITcpCommand { }
    
    [MessagePackObject]
    public class Fault : ITcpCommand
    {
        
        public Fault()
        {
        }
    }
}