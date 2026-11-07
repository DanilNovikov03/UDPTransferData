using UdpProgram.Udp;

namespace UdpProgram.CircularBuffer
{
    public class PacketCircularBuffer : MyCircularBuffer<UdpPacket>
    {
        protected uint _firstId;

        public PacketCircularBuffer(int capacity) : base(capacity)
        {
        }

        public override UdpPacket this[int index]
        {
            get => base[index];
            set => base[index] = value;
        }


        public override void Add(UdpPacket item) =>
            base.Add(item);

        public bool ContainsId(uint packetId)
        {
            return packetId >= _firstId 
                ? IndexOfId(packetId) >= 0 
                : false;
        }

        protected override void AddFullBuffer(UdpPacket item)
        {
            base.AddFullBuffer(item);
            _firstId++;
        }

        protected override void AddNoFullBuffer(UdpPacket item)
        {
            base.AddNoFullBuffer(item);

            if (IsEmpty)
                _firstId = item.PacketId;
        }

        protected int IndexOfId(uint packetId)
        {
            int position = (int)(packetId - _firstId);
            var packet = _buffer[InternalIndex(position)];

            if (packet != null && packet.PacketId == packetId)
                return position;

            return -1;
        }

        protected bool IsOldPacket(uint packetId) =>
            packetId < _firstId;
    }
}
