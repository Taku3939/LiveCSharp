using System;
using System.Linq;
namespace StreamServer.Model
{
    public class MinimumAvatarPacket
    {
        public readonly long PaketId;
        public readonly UnityEngine.Vector3 Position;
        public readonly float RadY;
        public readonly UnityEngine.Vector4 NeckRotation;
        public MinimumAvatarPacket(long paketId, UnityEngine.Vector3 position, float radY, UnityEngine.Vector4 neckRotation)
        {
            PaketId = paketId;
            Position = position;
            RadY = radY;
            NeckRotation = neckRotation;
            var time = (ulong) DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
    
    public class Option
    {
        public bool IsAbsolutely;

        public byte GetByte()
        {
            var b = IsAbsolutely ? '1' : '0';
            var padLeft = b.ToString().PadLeft(8, '0');
            return Convert.ToByte(padLeft, 2);
        }

        public static Option Parse(byte buf)
        {
            var body = Convert.ToString(buf, 2).PadLeft(8, '0').ToArray();
            var op = new Option();
            op.IsAbsolutely = body[7] == '1';
            return op;
        }
    }
}
