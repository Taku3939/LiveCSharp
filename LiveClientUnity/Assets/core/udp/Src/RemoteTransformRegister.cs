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
                    
                _queue.Enqueue(new Tuple<Vector3, Quaternion>(pos, rot));
            }
            
            while (_queue.Count > 1)
            {
                var buf = _queue.Dequeue();
                var buf2 = _queue.Dequeue();
             //   var pos = Mathf.Lerp(buf, buf2,);
                this.transform.position = buf.Item1;
                this.transform.rotation = buf.Item2;
               
            }
        }
    }
}
