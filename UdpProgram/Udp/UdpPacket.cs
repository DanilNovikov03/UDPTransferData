namespace UdpProgram.Udp
{
    public class UdpPacket
    {
        public uint PacketId { get; set; }
        public byte[] Data { get; set; }

        private static readonly int PacketIdByteSize = 4;


        public UdpPacket(uint id, byte[] data)
        {
            PacketId = id;
            Data = data;
        }


        public byte[] ToBytes() =>
            PacketToArrayBytes();
        
        public static UdpPacket FromBytes(byte[] bytes)
        {
            (uint packetId, byte[] data) = ExtractPacket(bytes);
            return new UdpPacket(packetId, data);
        }

        private byte[] PacketToArrayBytes()
        {
            List<byte> result = new List<byte>();

            byte[] packetIdBytes = BitConverter.GetBytes(PacketId);

            result.AddRange(packetIdBytes);
            result.AddRange(Data);

            return result.ToArray();
        }

        private static (uint packetId, byte[] data) ExtractPacket(byte[] bytes)
        {
            int sizeData = bytes.Length - PacketIdByteSize;

            uint packetId = BitConverter.ToUInt32(bytes, 0);
            byte[] segmentsData = new byte[sizeData];
            Buffer.BlockCopy(bytes, PacketIdByteSize, segmentsData, 0, segmentsData.Length);

            return (packetId, segmentsData);
        }
    }
}