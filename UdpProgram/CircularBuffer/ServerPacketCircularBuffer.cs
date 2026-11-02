using UdpProgram.Udp;

namespace UdpProgram.CircularBuffer
{
    public class ServerPacketCircularBuffer : MyCircularBuffer<UdpPacket>
    {
        // TODO возможен прыжок больше размера буффера
        // todo при добавлении в буффер используется InternalIndex возможно не стоит его каждый раз добавлять

        private UdpPacket _emptyPacket;
        private uint _firstId;

        public ServerPacketCircularBuffer(int capacity) : base(capacity)
        {
        }

        public override UdpPacket this[int index] 
        {
            get 
            {
                var packet = base[index];
                return packet ?? EmptyPacket;
            } 
            set => base[index] = value;
        }

        private UdpPacket EmptyPacket =>
            _emptyPacket ??= CreateEmptyPacket();


        public override void Add(UdpPacket packet)
        {
            if (IsOldPacket(packet))
                return;

            AddPacketToBuffer(packet);
        }

        public bool ContainsId(uint packetId) =>
            IndexOfId(packetId) >= 0;

        private bool IsOldPacket(UdpPacket packet) =>
            packet.PacketId < _firstId;

        private void AddPacketToBuffer(UdpPacket packet)
        {
            if (IsEmpty)
            {
                AddEmtyBuffer(packet);
                return;
            }

            int position =
                (int)(packet.PacketId - _firstId);

            if (CanInsertWithoutShifting(position))
                AddBufferFitsInWindow(position, packet);
            else
                AddBufferWithWindowShift(position, packet);
        }

        private bool CanInsertWithoutShifting(int position) =>
            position <= Count || !IsFull;

        private void AddEmtyBuffer(UdpPacket packet)
        {
            _buffer[_start] = packet;         
            _firstId = packet.PacketId;
            Count = 1;
            _end = InternalIndex(Count);          
        }

        private void AddBufferFitsInWindow(int position, UdpPacket packet)
        {
            if (ContainsId(packet.PacketId))
                return;

            int idx = InternalIndex(position);
            _buffer[idx] = packet;

            if (IsFull)
                AdvanceWindow();
            else if (position == Capacity && !IsFull)
                EnsureBufferFullState();
            else if (position == Count || !IsFull)
                RecalculateWindowState(position);
        }

        private void AdvanceWindow()
        {
            Increment(ref _end);
            _start = _end;
            _firstId++;
        }

        private void RecalculateWindowState(int position) =>
            _end = InternalIndex(++Count);

        private void EnsureBufferFullState()
        {
            Increment(ref _start);
            _end = _start;
            Count = Capacity;
            _firstId++;
        }

        // TODO rafactore code
        private void AddBufferWithWindowShift(int position, UdpPacket packet)
        {
            int newLen = position + 1;           

            if (newLen > Capacity)
            {
                int shift = newLen - Capacity;

                for (int i = 0; i < shift; i++)
                    _buffer[InternalIndex(i)] = null;

                _start = InternalIndex(shift);     
                _firstId += (uint)shift;           
                newLen = Capacity;                 
            }

            _buffer[InternalIndex(newLen - 1)] = packet;
            Count = newLen;
            _end = InternalIndex(Count);
        }

        private int IndexOfId(uint packetId)
        {
            int position = (int)(packetId - _firstId);
            var packet = _buffer[InternalIndex(position)];

            if (packet != null && packet.PacketId == packetId)
                return position;

            return -1;
        }

        private UdpPacket CreateEmptyPacket()
        {
            return new UdpPacket
            (
                0,
                Array.Empty<byte>()
            );
        }
    }
}
