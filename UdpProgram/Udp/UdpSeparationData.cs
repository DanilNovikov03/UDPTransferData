namespace UdpProgram.Udp
{
    public class UdpSeparationData
    {
        private static readonly int MaxSizeDataPacket = 65503;


        public static List<UdpPacket> SplitIntoPackets(byte[] data)
        {
            int totalPackets = GetPacketCount(data);
            return SeparationDataToPackets(data, totalPackets);
        }

        private static int GetPacketCount(byte[] data) =>
            (int)Math.Ceiling((double)data.Length / MaxSizeDataPacket);

        private static List<UdpPacket> SeparationDataToPackets(byte[] data, int countPackets)
        {
            List<UdpPacket> packets = new List<UdpPacket>();

            for (int packetId = 0; packetId < countPackets; packetId++)
            {
                int dataOffset = packetId * MaxSizeDataPacket;
                int packetSize = GetPacketSize(data.Length, dataOffset);

                if (packetSize > 0)
                {
                    UdpPacket packetDataUdp = BuildPacket(data, dataOffset, packetSize, (uint)packetId);
                    packets.Add(packetDataUdp);
                }
            }

            return packets;
        }

        private static int GetPacketSize(int dataLength, int dataOffset) =>
            Math.Min(MaxSizeDataPacket, dataLength - dataOffset);

        private static UdpPacket BuildPacket(byte[] data, int dataOffset, int packetSize, uint packetId)
        {
            byte[] packetData = new byte[packetSize];
            Buffer.BlockCopy(data, dataOffset, packetData, 0, packetSize);
            return new UdpPacket(packetId, packetData);
        }
    }
}
