using UdpProgram.Udp;

namespace UdpProgram.CircularBuffer
{
    public class ClientPacketCircularBuffer : PacketCircularBuffer
    {
        public ClientPacketCircularBuffer(int capacity) : base(capacity)
        {
        }


        public UdpPacket? TryGet(uint packetId)
        {
            if (packetId < _firstId)
                return null;

            int index = IndexOfId(packetId);

            return index < 0 ?
                null : _buffer[index];
        }
    }
}
