using StreamServer.Model;
using UnityEngine;
 using UnityEngine.Serialization;
 using Vector3 = UnityEngine.Vector3;

namespace StreamServer
{
    public class RemoteTransformRegister : MonoBehaviour
    {
        public long userId;
        [FormerlySerializedAs("modelManager")] [SerializeField] private DataHolder dataHolder;

        private void Update()
        {
            dataHolder.Users.TryGetValue(userId, out var user);
            var packet = user?.CurrentPacket;
            if (packet != null)
            {
                var pos = new Vector3(
                    packet.Position.X,
                    packet.Position.Y,
                    packet.Position.Z);
                var rot = new Quaternion(
                    packet.NeckRotation.X,
                    packet.NeckRotation.Y,
                    packet.NeckRotation.Z,
                    packet.NeckRotation.W);
                transform.localPosition = pos;
                transform.rotation = rot;
            }
        }
    }
}
