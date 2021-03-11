using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StreamServer.Model;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace StreamServer
{
    [CreateAssetMenu]
    public class RemotePlayerSpawner : ScriptableObject
    {
        [SerializeField] private GameObject remotePlayerPrefab;
        [NonSerialized] private List<GameObject> _remotePlayers = new List<GameObject>();
        [NonSerialized] public TaskScheduler TaskScheduler;
        public async Task Spawn(MinimumAvatarPacket packet)
        {
            var taskFactory = new TaskFactory();
            await taskFactory.StartNew(() =>
            {
                try
                {
                    remotePlayerPrefab.GetComponent<RemoteTransformRegister>().userId = packet.PaketId;
                    var go = Instantiate(remotePlayerPrefab,
                        new Vector3(packet.Position.X, packet.Position.Y, packet.Position.Z),
                        new Quaternion(packet.NeckRotation.X, packet.NeckRotation.Y, packet.NeckRotation.Z,
                            packet.NeckRotation.W));
                    go.GetComponent<ChangeCharacterData>().ChangeIcon(packet.PaketId);
                    _remotePlayers.Add(go);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
        }

        public async Task Remove(long userId)
        {
            var taskFactory = new TaskFactory();
            await taskFactory.StartNew(() =>
            {
                try
                {
                    var toRemove = _remotePlayers.Find(x => x.GetComponent<RemoteTransformRegister>().userId == userId);
                    _remotePlayers.Remove(toRemove);
                    Destroy(toRemove);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
            
        }
    }
}
