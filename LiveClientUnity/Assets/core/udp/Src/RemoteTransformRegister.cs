using System;
using System.Collections.Generic;
using StreamServer.Model;
using UnityEngine;
 using UnityEngine.Serialization;
 using Vector3 = UnityEngine.Vector3;

namespace StreamServer
{
    public class RemoteTransformRegister : MonoBehaviour
    {
        public long userId; 
        [SerializeField] private DataHolder dataHolder;
        private Queue<Tuple<Vector3, Quaternion>> _queue = new Queue<Tuple<Vector3, Quaternion>>();
        
        private Vector3 vec = new Vector3(0, 0, 0);
        private Quaternion q = Quaternion.identity;
        private float t = 0f;
        private void Update()
        {
            dataHolder.Users.TryGetValue(userId, out var user);
        
            var packet = user?.CurrentPacket;
            if (packet != null)
            {
                Debug.Log($"x : {packet.Position.x}, y : {packet.Position.y}, z : {packet.Position.z}");
                var pos = new Vector3(
                    packet.Position.x,
                    packet.Position.y,
                    packet.Position.z);
                var rot = new Quaternion(
                    packet.NeckRotation.x,
                    packet.NeckRotation.y,
                    packet.NeckRotation.z,
                    packet.NeckRotation.w);

                this.transform.position = pos;
               // this.transform.rotation = rot;
                // vec = pos;
                // q = rot;
            }
            
            // t += Time.deltaTime;
            // this.transform.position = Vector3.Lerp(this.transform.position, vec, Mathf.Clamp(t, 0f, 1f));
            // this.transform.rotation = Quaternion.Lerp(this.transform.rotation, q, Mathf.Clamp(t, 0f, 1f));
        }
    }
}
