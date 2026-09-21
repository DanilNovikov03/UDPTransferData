using UdpProgram.Protocol;

namespace UdpProgram.Udp
{
    public class UdpPacket
    {
        public uint PacketId { get; set; }
        public byte[] Data { get; set; }

        public UdpPacket(uint id, byte[] data)
        {
            PacketId = id;
            Data = data;
        }


        public byte[] ToBytes() =>
            UdpDataConverter.ToBytesPacket(this);
        
        public static UdpPacket FromBytes(byte[] bytes) =>
            UdpDataConverter.FromBytesToPacket(bytes);
    }
}