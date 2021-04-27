using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace VLLLiveEngine.test
{
    public class GameHub : StreamingHubBase<IGameHub, IGameHubReceiver>, IGameHub
    {
        private string roomName = "room";
        private Player self;
        IInMemoryStorage<Player> storage;
        private IGroup room;

        public async Task<Player[]> JoinAsync(ulong id)
        {
            Console.WriteLine("join");
            self = new Player() {Id = id};
            (room, storage) = await Group.AddAsync(roomName, self);
            Broadcast(room).OnJoin(id);
            return storage.AllValues.ToArray();
        }

        public async Task LeaveAsync(ulong id)
        {
            await room.RemoveAsync(this.Context);
            Broadcast(room).OnLeave(id);
        }
        
        protected override ValueTask OnDisconnected()
        {
            // on disconnecting, if automatically removed this connection from group.
            return CompletedTask;
        }
    }
    [MessagePackObject]
    public class Player
    {
        [Key(0)]
        public ulong Id{ get; set; }
    }
    
}