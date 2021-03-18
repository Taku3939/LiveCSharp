using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StreamServer.Model;
using UnityEngine;

namespace StreamServer
{
    public class Utility
    {
      public static string BufferToString(byte[] buf)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < buf.Length; ++i)
            {
                sb.Append(buf[i]);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");
            return sb.ToString();
        }


        public static MinimumAvatarPacket BufferToPacket(byte[] buf)
        {
            if (buf == null || buf.Length != 20) return null;
            var userId = BitConverter.ToInt64(buf, 4);
            var offset = 12;
            var x = PacketUtil.ConvertFloat(buf[offset], 1);
            var y = PacketUtil.ConvertFloat(buf[offset + 1], 1);
            var z = PacketUtil.ConvertFloat(buf[offset + 2], 1);
            var radY = PacketUtil.ConvertFloat(buf[offset + 3], 1);
            var qx = PacketUtil.ConvertFloat(buf[offset + 4], 1);
            var qy = PacketUtil.ConvertFloat(buf[offset + 5], 1);
            var qz = PacketUtil.ConvertFloat(buf[offset + 6], 1);
            var qw = PacketUtil.ConvertFloat(buf[offset + 7], 1);

            MinimumAvatarPacket packet =
                new MinimumAvatarPacket(userId, new Vector3(x, y, z), radY, new Vector4(qx, qy, qz, qw));
            return packet;
        }

        public static List<MinimumAvatarPacket> BufferToPackets(byte[] buf)
        {
            if (buf != null && buf.Length > 0)
            {
                int begin = sizeof(float);
                var numPackets = BitConverter.ToInt32(buf, 0);
                var supposedBufSize = numPackets * 16 + begin;
                if (buf.Length == supposedBufSize)
                {
                    List<MinimumAvatarPacket> packets = new List<MinimumAvatarPacket>();
                    for (int i = 0; i < numPackets; ++i)
                    {
                        var beginOffset = i * 16 + begin;
                        var userId = BitConverter.ToInt64(buf, beginOffset);
                        beginOffset += sizeof(long);
                        var x = PacketUtil.ConvertFloat(buf[beginOffset], 1);
                        var y = PacketUtil.ConvertFloat(buf[beginOffset + 1], 1);
                        var z = PacketUtil.ConvertFloat(buf[beginOffset + 2], 1);
                        var radY = PacketUtil.ConvertFloat(buf[beginOffset + 3], 1);
                        var qx = PacketUtil.ConvertFloat(buf[beginOffset + 4], 1);
                        var qy = PacketUtil.ConvertFloat(buf[beginOffset + 5], 1);
                        var qz = PacketUtil.ConvertFloat(buf[beginOffset + 6], 1);
                        var qw = PacketUtil.ConvertFloat(buf[beginOffset + 7], 1);
                        MinimumAvatarPacket packet = new MinimumAvatarPacket(userId, new Vector3(x, y, z), radY,
                            new Vector4(qx, qy, qz, qw));
                        packets.Add(packet);
                    }

                    return packets;
                }
            }


            return null;
        }

        public static byte[] PacketToBuffer(MinimumAvatarPacket packet)
        {
            byte[] buff = new byte[20];
            var numPackets = BitConverter.GetBytes(1);
            Buffer.BlockCopy(numPackets, 0, buff, 0, sizeof(int));
            var beginOffset = 0;
            var id = BitConverter.GetBytes(packet.PaketId);
            System.Console.WriteLine(id.Length);
            var bx = PacketUtil.ConvertByte(packet.Position.x, 1);
            var by = PacketUtil.ConvertByte(packet.Position.y, 1);
            var bz = PacketUtil.ConvertByte(packet.Position.z, 1);
            var bRadY = PacketUtil.ConvertByte(packet.RadY, 1);
            var bQx = PacketUtil.ConvertByte(packet.NeckRotation.x, 1);
            var bQy = PacketUtil.ConvertByte(packet.NeckRotation.y, 1);
            var bQz = PacketUtil.ConvertByte(packet.NeckRotation.z, 1);
            var bQw = PacketUtil.ConvertByte(packet.NeckRotation.w, 1);
            byte[] body = {bx, by, bz, bRadY, bQx, bQy, bQz, bQw};
            Buffer.BlockCopy(id, 0, buff, 4, id.Length);
            Buffer.BlockCopy(body, 0, buff, 12, body.Length);
            return buff;
        }

        public static byte[] PacketsToBuffer(List<MinimumAvatarPacket> packets)
        {
            int offset = 4;
            byte[] buff = new byte[16 * packets.Count + offset];
            var numPackets = BitConverter.GetBytes(packets.Count);
            Buffer.BlockCopy(numPackets, 0, buff, 0, sizeof(int));
            for (int i = 0; i < packets.Count; ++i)
            {
                var packet = packets[i];
                var beginOffset = i * 16;
                var id = BitConverter.GetBytes(packet.PaketId);
                var bx = PacketUtil.ConvertByte(packet.Position.x, 1);
                var by = PacketUtil.ConvertByte(packet.Position.y, 1);
                var bz = PacketUtil.ConvertByte(packet.Position.z, 1);
                var bRadY = PacketUtil.ConvertByte(packet.RadY, 1);
                var bQx = PacketUtil.ConvertByte(packet.NeckRotation.x, 1);
                var bQy = PacketUtil.ConvertByte(packet.NeckRotation.y, 1);
                var bQz = PacketUtil.ConvertByte(packet.NeckRotation.z, 1);
                var bQw = PacketUtil.ConvertByte(packet.NeckRotation.w, 1);
                byte[] body = {bx, by, bz, bRadY, bQx, bQy, bQz, bQw};
                Buffer.BlockCopy(id, 0, buff, offset + beginOffset, id.Length);
                Buffer.BlockCopy(body, 0, buff, offset + beginOffset + sizeof(long), body.Length);
            }

            return buff;
        }

        public static List<byte[]> PacketsToBuffers(List<MinimumAvatarPacket> packets)
        {
            var packetsList = new List<List<MinimumAvatarPacket>>();
            const int nSize = 29;
            for (int i = 0; i < packets.Count; i += nSize)
            {
                packetsList.Add(packets.GetRange(i, Math.Min(nSize, packets.Count - i)));
            }

            var buffersList = new List<byte[]>();
            foreach (var pcs in packetsList)
            {
                buffersList.Add(PacketsToBuffer(pcs));
            }

            return buffersList;
        }

        
        public static void PrintDbg<T>(T str, object sender = null)
        {
            Debug.Log($"[{sender}] {str}");
        }
    }
}