using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamServer;
using StreamServer.Model;
using UnityEngine;

namespace Udp
{
    public class SendEntry : MonoBehaviour
    {
        [SerializeField] private UdpSocketHolder udpSocketHolder;
        [SerializeField] private DataHolder _dataHolder;
        [SerializeField] private Transform target, origin;
        private SyncOutputLoop output;
        private const float Range = 127f;

        private Task delay = Task.Delay(100);

        async Task OnEnable()
        {
            try
            {
                await delay;

                //Originタグのついたゲームオブジェクトを検索
                if (origin == null) origin = GameObject.FindWithTag("Origin").transform;

                //Udp通信の開始
                output = new SyncOutputLoop(udpSocketHolder.UdpClient, udpSocketHolder, _dataHolder.selfId);
                output.Start();
                output.TransformList.Add(transform);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private async void OnDisable()
        {
            await delay;
            await Task.Delay(10);
            udpSocketHolder.TryClose();
        }


        private async Task Update()
        {
            await delay;

            TimeSpan timeSpan = DateTime.Now.Subtract(new DateTime(1970, 1, 1));
            var totalTime = Convert.ToDouble(timeSpan.TotalMilliseconds);
            var buff = Utility.PacketsToBuffer(new List<MinimumAvatarPacket>
            {
                new MinimumAvatarPacket(
                    _dataHolder.selfId,
                    GetPosition(target.position, origin.position),
                    0,
                    target.rotation, totalTime + 2000)

            });

            output.Send(buff);
        }

        private static Vector3 GetPosition(Vector3 pos, Vector3 origin)
        {
            var x = Mathf.Clamp(pos.x - origin.x, -Range, Range);
            var y = Mathf.Clamp(pos.y - origin.y, -Range, Range);
            var z = Mathf.Clamp(pos.z - origin.z, -Range, Range);
            return new Vector3(x, y, z);
        }

    }
}