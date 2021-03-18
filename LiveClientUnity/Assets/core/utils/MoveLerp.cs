using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;

public class MoveLerp : MonoBehaviour
{
    private Queue<Message> _queue = new Queue<Message>();

    [SerializeField] private Transform _transform;
    public float interval = 0.1f;

    private Position _position;
    private void Start()
    {
        //新しい目的地の追加
        Observable.Interval(TimeSpan.FromSeconds(interval)).Subscribe(_ =>
        {
            //var pos = _position.GetPosition();
            //var rot = _position.GetRotation();
            // _queue.Enqueue(new (pos, rot));
        });
    }

    private Vector3 vec = new Vector3(0, 0, 0);
    private Quaternion q = Quaternion.identity;
    private float t = 0f;

    private void Update()
    {
        while (_queue.Count != 0)
        {
            var deq = _queue.Dequeue();
            // vec = deq.;
            // q = deq.q;
            t = 0f;
        }

        t += Time.deltaTime;
        this.transform.position = Vector3.Lerp(this.transform.position, vec, Mathf.Clamp(t, 0f, 1f));
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, q, Mathf.Clamp(t, 0f, 1f));
    }

    public float force;
}

public class Message
{
    public readonly long PaketId;
    public readonly Vector3 Position;
    public readonly float RadY;
    public readonly Vector4 NeckRotation;

    public Message(long paketId, Vector3 position, float radY, Vector4 neckRotation)
    {
        PaketId = paketId;
        Position = position;
        //RadY = radY;
        //BitArray array = new BitArray(5, );
        NeckRotation = neckRotation;
        var time = (ulong) DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    void Send()
    {
        byte[] buf = new byte[20];

    }
}