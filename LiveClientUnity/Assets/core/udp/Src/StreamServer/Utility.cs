using System;
using System.Collections.Generic;
using System.Text;
using StreamServer.Model;
using UnityEngine;
using Vector3 = StreamServer.Model.Vector3;
using Vector4 = StreamServer.Model.Vector4;

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
            if (buf == null || buf.Length != 64) return null;
            var idLen = sizeof(long);
            byte[] bStr = new byte[8];
            Buffer.BlockCopy(buf, 16 + 1, bStr, 0, idLen);
            var userId = BitConverter.ToInt64(bStr, 0);
            var x = BitConverter.ToSingle(buf, 32);
            var y = BitConverter.ToSingle(buf, 32 + sizeof(float));
            var z = BitConverter.ToSingle(buf, 32 + sizeof(float) * 2);
            var radY = BitConverter.ToSingle(buf, 32 + sizeof(float) * 3);
            var qx = BitConverter.ToSingle(buf, 48);
            var qy = BitConverter.ToSingle(buf, 48 + sizeof(float));
            var qz = BitConverter.ToSingle(buf, 48 + sizeof(float) * 2);
            var qw = BitConverter.ToSingle(buf, 48 + sizeof(float) * 3);
            MinimumAvatarPacket packet =
                new MinimumAvatarPacket(userId, new Vector3(x, y, z), radY, new Vector4(qx, qy, qz, qw));
            return packet;
        }

        public static List<MinimumAvatarPacket> BufferToPackets(byte[] buf)
        {
            if (buf != null && buf.Length > 0)
            {
                var numPackets = BitConverter.ToInt32(buf, 0);
                var supposedBufSize = numPackets * 48 + 16;
                if (buf.Length == supposedBufSize)
                {
                    List<MinimumAvatarPacket> packets = new List<MinimumAvatarPacket>();
                    for (int i = 0; i < numPackets; ++i)
                    {
                        var beginOffset = i * 48;
                        var userId = BitConverter.ToInt64(buf, 17 + beginOffset);
                        var x = BitConverter.ToSingle(buf, 32 + beginOffset);
                        var y = BitConverter.ToSingle(buf, 32 + sizeof(float) + beginOffset);
                        var z = BitConverter.ToSingle(buf, 32 + sizeof(float) * 2 + beginOffset);
                        var radY = BitConverter.ToSingle(buf, 32 + sizeof(float) * 3 + beginOffset);
                        var qx = BitConverter.ToSingle(buf, 48 + beginOffset);
                        var qy = BitConverter.ToSingle(buf, 48 + sizeof(float) + beginOffset);
                        var qz = BitConverter.ToSingle(buf, 48 + sizeof(float) * 2 + beginOffset);
                        var qw = BitConverter.ToSingle(buf, 48 + sizeof(float) * 3 + beginOffset);
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
            byte[] buff = new byte[64];
            var numPackets = BitConverter.GetBytes(1);
            Buffer.BlockCopy(numPackets, 0, buff, 0, sizeof(int));
            var beginOffset = 0;
            var id = BitConverter.GetBytes(packet.PaketId);
            System.Console.WriteLine(id.Length);
            var bx = BitConverter.GetBytes(packet.Position.X);
            var by = BitConverter.GetBytes(packet.Position.Y);
            var bz = BitConverter.GetBytes(packet.Position.Z);
            var bRadY = BitConverter.GetBytes(packet.RadY);
            var bQx = BitConverter.GetBytes(packet.NeckRotation.X);
            var bQy = BitConverter.GetBytes(packet.NeckRotation.Y);
            var bQz = BitConverter.GetBytes(packet.NeckRotation.Z);
            var bQw = BitConverter.GetBytes(packet.NeckRotation.W);
            buff[16] = sizeof(long);
            Buffer.BlockCopy(id, 0, buff, 16 + 1, id.Length);
            Buffer.BlockCopy(bx, 0, buff, 32, sizeof(float));
            Buffer.BlockCopy(by, 0, buff, 32 + sizeof(float), sizeof(float));
            Buffer.BlockCopy(bz, 0, buff, 32 + sizeof(float) * 2, sizeof(float));
            Buffer.BlockCopy(bRadY, 0, buff, 32 + sizeof(float) * 3, sizeof(float));
            Buffer.BlockCopy(bQx, 0, buff, 48, sizeof(float));
            Buffer.BlockCopy(bQy, 0, buff, 48 + sizeof(float), sizeof(float));
            Buffer.BlockCopy(bQz, 0, buff, 48 + sizeof(float) * 2, sizeof(float));
            Buffer.BlockCopy(bQw, 0, buff, 48 + sizeof(float) * 3, sizeof(float));
            return buff;
        }

        public static byte[] PacketsToBuffer(List<MinimumAvatarPacket> packets)
        {
            byte[] buff = new byte[48 * packets.Count + 16];
            var numPackets = BitConverter.GetBytes(packets.Count);
            Buffer.BlockCopy(numPackets, 0, buff, 0, sizeof(int));
            for (int i = 0; i < packets.Count; ++i)
            {
                var packet = packets[i];
                var beginOffset = i * 48;
                var id = BitConverter.GetBytes(packet.PaketId);
                var bx = BitConverter.GetBytes(packet.Position.X);
                var by = BitConverter.GetBytes(packet.Position.Y);
                var bz = BitConverter.GetBytes(packet.Position.Z);
                var bRadY = BitConverter.GetBytes(packet.RadY);
                var bQx = BitConverter.GetBytes(packet.NeckRotation.X);
                var bQy = BitConverter.GetBytes(packet.NeckRotation.Y);
                var bQz = BitConverter.GetBytes(packet.NeckRotation.Z);
                var bQw = BitConverter.GetBytes(packet.NeckRotation.W);
                buff[16 + beginOffset] = (byte) id.Length;
                Buffer.BlockCopy(id, 0, buff, 16 + 1 + beginOffset, id.Length);
                Buffer.BlockCopy(bx, 0, buff, 32 + beginOffset, sizeof(float));
                Buffer.BlockCopy(by, 0, buff, 32 + sizeof(float) + beginOffset, sizeof(float));
                Buffer.BlockCopy(bz, 0, buff, 32 + sizeof(float) * 2 + beginOffset, sizeof(float));
                Buffer.BlockCopy(bRadY, 0, buff, 32 + sizeof(float) * 3 + beginOffset, sizeof(float));
                Buffer.BlockCopy(bQx, 0, buff, 48 + beginOffset, sizeof(float));
                Buffer.BlockCopy(bQy, 0, buff, 48 + sizeof(float) + beginOffset, sizeof(float));
                Buffer.BlockCopy(bQz, 0, buff, 48 + sizeof(float) * 2 + beginOffset, sizeof(float));
                Buffer.BlockCopy(bQw, 0, buff, 48 + sizeof(float) * 3 + beginOffset, sizeof(float));
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