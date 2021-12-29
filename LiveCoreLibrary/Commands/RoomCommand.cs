using System;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    [MessagePackObject]
    public class Join : ITcpCommand
    {
        [Key(0)] public Guid Guid { get; }

        public Join(Guid guid)
        {
            this.Guid = guid;
        }
    }

    [MessagePackObject]
    public class Leave : ITcpCommand
    {
        [Key(0)] public Guid Guid { get; }

        public Leave(Guid guid)
        {
            this.Guid = guid;
        }
    }
}