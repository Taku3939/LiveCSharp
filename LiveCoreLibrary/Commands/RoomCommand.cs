using System;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class Join : ITcpCommand
    {
        [Key(0)] public Guid UserId { get; }

        [Key(1)] public string RoomName { get; }
        public Join(Guid userId, string roomName)
        {
            this.UserId = userId;
            this.RoomName = roomName;
        }
    }

    [MessagePackObject]
    public class Leave : ITcpCommand
    {
        [Key(0)] public Guid UserId { get; }

        public Leave(Guid userId)
        {
            this.UserId = userId;
        }
    }
    
    [MessagePackObject]
    public class JoinResult : ITcpCommand
    {
        [Key(0)] public Guid UserId;
        public JoinResult(Guid userId)
        {
            this.UserId = userId;
        }
    }
    
    [MessagePackObject]
    public class LeaveResult : ITcpCommand
    {
        [Key(0)] public Guid UserId;
        
        public LeaveResult(Guid userId)
        {
            this.UserId = userId;
        }
    }

    /// <summary>
    /// 送信できない
    /// </summary>
    public class Disconnect : ITcpCommand { }
}