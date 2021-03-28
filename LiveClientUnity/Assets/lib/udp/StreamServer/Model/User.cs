namespace StreamServer.Model
{
    public class User
    {
        public readonly ulong UserId;
        public volatile bool IsConnected;
        public volatile MinimumAvatarPacket CurrentPacket;
        public volatile DateTimeBox DateTimeBox;

        public User(ulong userId)
        {
            UserId = userId;
        }
        
        public User(User instance)
        {
            UserId = instance.UserId;
            IsConnected = instance.IsConnected;
            DateTimeBox = instance.DateTimeBox;
        }
    }
}
