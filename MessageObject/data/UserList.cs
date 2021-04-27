using MessagePack;

namespace MessageObject
{
    [MessagePackObject]
    public class UserList
    {
        [Key(0)]
        public ulong[] Users { get; }

        public UserList(ulong[] users)
        {
            this.Users = users;
        }
    }
}