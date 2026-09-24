using System.Text;
using UdpProgram.Udp;

namespace UdpProgram.Protocol
{
    public static class UdpDataConverter
    {
        public static byte[] StringToBytes(string str) =>
            Encoding.UTF8.GetBytes(str);

        public static string BytesToString(byte[] bytes) =>
            Encoding.UTF8.GetString(bytes);

        // todo: Rewrite from the sheet directly to array byte[].
        public static byte[] ToBytesPacket(UdpPacket packet)
        {
            List<byte> result = new List<byte>();

            byte[] packetIdBytes = BitConverter.GetBytes(packet.PacketId);

            result.AddRange(packetIdBytes);
            result.AddRange(packet.Data);

            return result.ToArray();
        }

        public static UdpPacket FromBytesToPacket(byte[] bytes)
        {
            uint packetId = BitConverter.ToUInt32(bytes, 0);
            byte[] segmentsData = new byte[bytes.Length - UdpProtocolConstant.SizeIdPacket];
            Buffer.BlockCopy(bytes, 4, segmentsData, 0, segmentsData.Length);

            return new UdpPacket(packetId, segmentsData);
        }

        public static string RemovePrefix(string input, string prefix) =>
            input.Substring(prefix.Length).Trim();

        public static List<uint> ParseLostIPackets(string lostIdPacketsMessage)
        {
            return lostIdPacketsMessage
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(uint.Parse)
                    .ToList();
        }
    }
}
