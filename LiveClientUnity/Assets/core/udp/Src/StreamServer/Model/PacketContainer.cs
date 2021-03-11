using System;

namespace StreamServer.Model
{
    public class PacketContainer
    {
        private readonly object lockObject = new object();
        private MinimumAvatarPacket _currentMinimumAvatarPacket;

        public MinimumAvatarPacket CurrentMinimumAvatarPacket
        {
            get
            {
                lock(lockObject)
                {
                    return _currentMinimumAvatarPacket;
                }
            }
            set
            {
                lock (lockObject)
                {
                    _currentMinimumAvatarPacket = value ?? throw new ArgumentNullException(nameof(value));
                }
            }
        }
    }
}
