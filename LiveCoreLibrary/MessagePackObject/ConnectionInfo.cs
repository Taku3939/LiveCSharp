using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class ConnectionInfo
    {
        public ulong Id;
        public ConnectionType ConnectionType;

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