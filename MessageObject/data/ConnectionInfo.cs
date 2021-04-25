using MessagePack;

namespace MessageObject
{
    [MessagePackObject]
    public class ConnectionInfo
    {
        [Key(0)] public ulong Id { get; }
        [Key(1)] public ConnectionType ConnectionType { get; }

        public ConnectionInfo(ulong id, ConnectionType connectionType)
        {
            this.Id = id;
            this.ConnectionType = connectionType;
        }
    }

    public enum ConnectionType
    {
        JOIN, LEAVE
    }
}