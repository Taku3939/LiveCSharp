using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamServer;
using StreamServer.Model;
using UnityEngine;

public class SendEntry : MonoBehaviour
{
    [SerializeField] private UdpSocketHolder udpSocketHolder;
    [SerializeField] private DataHolder _dataHolder;
    private SyncOutputLoop output;
    private Task delay = Task.Delay(100);
    async Task OnEnable()
    {
        try
        {
            await delay;
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

   public Position _position;
    private async Task Update()
    {
        await delay;
        //var position = transform.localPosition;
        //var rotation = transform.rotation;
        var buff = Utility.PacketsToBuffer(new List<MinimumAvatarPacket>{new MinimumAvatarPacket(
            _dataHolder.selfId,
            _position.GetPosition(this.transform.localPosition),
            _position.GetRadY(),
            new Vector4(0, 0, 0, 0))});
        
        // PacketUtil.ConvertFloat(buff[beginOffset], 1); 
        // //var buf = Utility.BufferToPackets(buff);
        // // foreach (var b in buf)
        // // {
        // //     Debug.Log(b.Position.x);
        // //     Debug.Log(b.Position.y);
        // //     Debug.Log(b.Position.z);
        // // }
        output.Send(buff);
    }
}
