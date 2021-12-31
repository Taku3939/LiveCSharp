using System;
using System.Collections.Concurrent;
using LiveCoreLibrary;
using LiveCoreLibrary.Commands;
using LiveCoreLibrary.Utility;

namespace LiveServer
{
    public class RoomHolder : IObserver<ReceiveData>
    {
        private readonly ConcurrentDictionary<string, Room> _rooms = new ConcurrentDictionary<string, Room>();

        public void OnNext(ReceiveData value)
        {
            ITcpCommand tcpCommand = value.TcpCommand;
           
            switch (tcpCommand)
            {
                case Join x:
                    // 別の部屋にいたら退出させる
                    _rooms.Others(x.RoomName).ForEach(y => y.Remove(x.UserId));
                    // 新しい部屋に割り振る 
                    _rooms.Get(x.RoomName)?.Add(x.UserId, value.Client);
                    break;

                case Leave x:
                    // すべての部屋に特定ユーザの退出命令
                    foreach (var room in _rooms.Values)
                        room.Remove(x.UserId);
                    break;
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }


        public void Add(Room room)
        {
            if (this._rooms.TryAdd(room.Name, room)) { }
        }
        public void Remove(string roomName)
        {
            if (this._rooms.TryRemove(roomName, out var room)) { }
        }
    }
}