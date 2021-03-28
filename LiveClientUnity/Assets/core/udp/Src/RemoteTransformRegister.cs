using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StreamServer.Model;
using UniRx;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Udp
{
    public class RemoteTransformRegister : MonoBehaviour
    {
        public ulong userId; 
        [SerializeField] private DataHolder dataHolder;
        private readonly Queue<MinimumAvatarPacket> _queue = new Queue<MinimumAvatarPacket>();

        private MinimumAvatarPacket _start, _dist;
        private float interval = 1.0f;
        private double _totalTime;
        private void Start()
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(new DateTime(1970, 1, 1));
            _totalTime = Convert.ToDouble(timeSpan.TotalMilliseconds);
            Observable.Interval(TimeSpan.FromSeconds(interval)).Subscribe(_ =>
            {
                dataHolder.Users.TryGetValue(userId, out var user);
                var packet = user?.CurrentPacket;
                if(packet == null) return;
                _queue.Enqueue(packet);
            });
        }

        private void Update()
        {
            TimeSpan timeSpan = DateTime.Now.Subtract(new DateTime(1970, 1, 1));
            _totalTime = Convert.ToDouble(timeSpan.TotalMilliseconds);

            if (_queue.Count > 10)
            {
                Debug.Log("メッセージの量が多すぎる");
                _queue.Clear();
            }
            while (_queue.Count != 0)
            {
                
                if (_dist != null && _totalTime < _dist.Time) break;
                _start = _dist;
                _dist = _queue.Dequeue();
            }

            if(_start == null || _dist == null) return;
            var rate = InverseLerp(_start.Time, _dist.Time, _totalTime);
            var pos = Vector3.Lerp(_start.Position, _dist.Position, ThirdOrderInterpolation(rate));
            var rot = Quaternion.Lerp(_start.NeckRotation, _dist.NeckRotation, rate);
            this.transform.position = pos;
            this.transform.rotation = rot;
        }


        public static float ThirdOrderInterpolation(float t) => t * t * (3 - 2 * t);

        public static float CosInterpolation(float t) =>(float) (1f - Math.Cos(t * Mathf.PI)) / 2f;
        public static float InverseLerp(double a, double b, double value)
        {
            if (b == a) return 0;
            var v = (value - a) / (b - a);
            return Mathf.Clamp01((float) v);
        }
    }
}
