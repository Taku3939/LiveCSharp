using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using StreamServer;
using StreamServer.Model;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Udp
{
    [CreateAssetMenu]
    public class RemotePlayerSpawner : ScriptableObject, IPlayerSpawner
    {
        [SerializeField] private GameObject remotePlayerPrefab;
        [NonSerialized] private ConcurrentDictionary<ulong, GameObject> _remotePlayers = new ConcurrentDictionary<ulong, GameObject>();
        [NonSerialized] public TaskScheduler TaskScheduler;
        public async Task Spawn(MinimumAvatarPacket packet)
        {
            var taskFactory = new TaskFactory();
            await taskFactory.StartNew(() =>
            {
                try
                {
                    if (!_remotePlayers.ContainsKey(packet.PaketId))
                    {
                        remotePlayerPrefab.GetComponent<RemoteTransformRegister>().userId = packet.PaketId;
                        var go = Instantiate(remotePlayerPrefab,
                            new Vector3(packet.Position.x, packet.Position.y, packet.Position.z),
                            new Quaternion(packet.NeckRotation.x, packet.NeckRotation.y, packet.NeckRotation.z,
                                packet.NeckRotation.w));
                        go.GetComponent<ChangeCharacterData>().ChangeIcon(packet.PaketId);
                        _remotePlayers.TryAdd(packet.PaketId, go);
                    }
     
                 
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
        }

        public async Task Remove(ulong userId)
        {
            var taskFactory = new TaskFactory();
            await taskFactory.StartNew(() =>
            {
                try
                {
                    _remotePlayers.TryRemove(userId, out var go);
                    Destroy(go);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
            
        }
    }
}
