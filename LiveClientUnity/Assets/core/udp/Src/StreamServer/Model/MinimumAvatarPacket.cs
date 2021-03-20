using System;
using System.Linq;
namespace StreamServer.Model
{
    public class MinimumAvatarPacket
    {
        public readonly ulong PaketId;
        public readonly UnityEngine.Vector3 Position;
        public readonly float RadY;
        public readonly UnityEngine.Quaternion NeckRotation;
        public readonly double time;
        public MinimumAvatarPacket(ulong paketId, UnityEngine.Vector3 position, float radY, UnityEngine.Quaternion neckRotation, double time)
        {
            this.PaketId = paketId;
            this.Position = position;
            this.RadY = radY;
            this.NeckRotation = neckRotation;
            this.time = time;
        }
    }
}
