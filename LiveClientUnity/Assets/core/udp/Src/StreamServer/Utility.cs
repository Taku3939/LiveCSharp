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
            if (buf == null || buf.Length != 31) return null;
            var offset = sizeof(float);
            var userId = BitConverter.ToUInt64(buf, offset);
            var x = BitConverter.ToInt16(buf, offset += sizeof(long));
            var y = BitConverter.ToInt16(buf, offset += sizeof(short));
            var z = BitConverter.ToInt16(buf, offset += sizeof(short));
            offset += sizeof(short);
            var radY = PacketUtil.ConvertInt(buf[offset]);
            var qx = PacketUtil.ConvertInt(buf[offset + 1]);
            var qy = PacketUtil.ConvertInt(buf[offset + 2]);
            var qz = PacketUtil.ConvertInt(buf[offset + 3]);
            var qw = PacketUtil.ConvertInt(buf[offset + 4]);
            var time = BitConverter.ToDouble(buf, offset + 5);
            MinimumAvatarPacket packet =
                new MinimumAvatarPacket(userId, new Vector3(x, y, z) / 10, radY /100f, new Quaternion(qx /100f, qy /100f, qz /100f, qw /100f), time);
            return packet;
        }

        public static List<MinimumAvatarPacket> BufferToPackets(byte[] buf)
        {
            if (buf != null && buf.Length > 0)
            {
                int begin = sizeof(float);
                var numPackets = BitConverter.ToInt32(buf, 0);
                var supposedBufSize = numPackets * 27 + begin;
                if (buf.Length == supposedBufSize)
                {
                    List<MinimumAvatarPacket> packets = new List<MinimumAvatarPacket>();
                    for (int i = 0; i < numPackets; ++i)
                    {
                        var offset = begin + i * 27;
                        var userId = BitConverter.ToUInt64(buf, offset);
                        var x = BitConverter.ToInt16(buf, offset += sizeof(long));
                        var y = BitConverter.ToInt16(buf, offset += sizeof(short));
                        var z = BitConverter.ToInt16(buf, offset+= sizeof(short));
                        offset += sizeof(short);
                        var radY = PacketUtil.ConvertInt(buf[offset]);
                        var qx = PacketUtil.ConvertInt(buf[offset + 1]);
                        var qy = PacketUtil.ConvertInt(buf[offset + 2]);
                        var qz = PacketUtil.ConvertInt(buf[offset + 3]);
                        var qw = PacketUtil.ConvertInt(buf[offset + 4]);
                        var time = BitConverter.ToDouble(buf, offset + 5);
                        MinimumAvatarPacket packet = new MinimumAvatarPacket(userId, new Vector3(x, y, z) / 10, radY / 100f,
                            new Quaternion(qx / 100f, qy / 100f, qz /100f, qw /100f), time);
                        packets.Add(packet);
                    }

                    return packets;
                }
            }


            return null;
        }

        public static byte[] PacketToBuffer(MinimumAvatarPacket packet)
        {
            byte[] buff = new byte[31];
            var numPackets = BitConverter.GetBytes(1);
            Buffer.BlockCopy(numPackets, 0, buff, 0, sizeof(int));
            var id = BitConverter.GetBytes(packet.PaketId);
            var bx = BitConverter.GetBytes((short) (packet.Position.x * 10f));
            var by = BitConverter.GetBytes((short) (packet.Position.y * 10f));
            var bz = BitConverter.GetBytes((short) (packet.Position.z * 10f));
            var bRadY = PacketUtil.ConvertByte((int) (packet.RadY * 100f));
            var bQx = PacketUtil.ConvertByte((int) (packet.NeckRotation.x * 100f));
            var bQy = PacketUtil.ConvertByte((int) (packet.NeckRotation.y * 100f));
            var bQz = PacketUtil.ConvertByte((int) (packet.NeckRotation.z * 100f));
            var bQw = PacketUtil.ConvertByte((int) (packet.NeckRotation.w * 100f));
            var time = BitConverter.GetBytes(packet.time);
            byte[] body = {bRadY, bQx, bQy, bQz, bQw};
            Buffer.BlockCopy(id, 0, buff, 4, id.Length);
            Buffer.BlockCopy(bx, 0, buff, 12, bx.Length);
            Buffer.BlockCopy(by, 0, buff, 14, by.Length);
            Buffer.BlockCopy(bz, 0, buff, 16, bz.Length);
            Buffer.BlockCopy(body, 0, buff, 18, body.Length);
            Buffer.BlockCopy(time, 0, buff, 23 ,time.Length);
            return buff;
        }

        public static byte[] PacketsToBuffer(List<MinimumAvatarPacket> packets)
        {
            int offset = 4;
            byte[] buff = new byte[27 * packets.Count + offset];
            var numPackets = BitConverter.GetBytes(packets.Count);
            Buffer.BlockCopy(numPackets, 0, buff, 0, sizeof(int));
            for (int i = 0; i < packets.Count; ++i)
            {
                var packet = packets[i];
                offset = 4 + i * 27;
                var id = BitConverter.GetBytes(packet.PaketId);
                var bx = BitConverter.GetBytes((short) (packet.Position.x * 10f));
                var by = BitConverter.GetBytes((short) (packet.Position.y * 10f));
                var bz = BitConverter.GetBytes((short) (packet.Position.z * 10f));
                var bRadY = PacketUtil.ConvertByte((int) (packet.RadY * 100f));
                var bQx = PacketUtil.ConvertByte((int) (packet.NeckRotation.x * 100f));
                var bQy = PacketUtil.ConvertByte((int) (packet.NeckRotation.y * 100f));
                var bQz = PacketUtil.ConvertByte((int) (packet.NeckRotation.z * 100f));
                var bQw = PacketUtil.ConvertByte((int) (packet.NeckRotation.w * 100f));
                var time = BitConverter.GetBytes(packet.time);
                byte[] body = {bRadY, bQx, bQy, bQz, bQw};
                Buffer.BlockCopy(id, 0, buff, offset, id.Length);
                Buffer.BlockCopy(bx, 0, buff, offset += id.Length, bx.Length);
                Buffer.BlockCopy(by, 0, buff, offset += bx.Length, by.Length);
                Buffer.BlockCopy(bz, 0, buff, offset += by.Length, bz.Length);
                Buffer.BlockCopy(body, 0, buff, offset += bz.Length, body.Length);
                Buffer.BlockCopy(time, 0, buff, offset += body.Length, time.Length);
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