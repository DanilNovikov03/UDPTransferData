using UdpProgram.Udp;

namespace UdpProgram.CircularBuffer
{
    public class ServerPacketCircularBuffer : PacketCircularBuffer
    {
        // TODO возможен прыжок больше размера буффера
        // todo при добавлении в буффер используется InternalIndex возможно не стоит его каждый раз добавлять

        private UdpPacket _emptyPacket;

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
            if (IsOldPacket(packet.PacketId))
                return;

            AddPacketToBuffer(packet);
        }

        public IReadOnlyList<UdpPacket> AddAndEvicted(UdpPacket packet)
        {
            if (IsOldPacket(packet.PacketId))
                return ListArrayEmpty();

            return AddPacketToBuffer(packet);
        }


        private IReadOnlyList<UdpPacket> AddPacketToBuffer(UdpPacket packet)
        {
            if (IsEmpty)
            {
                AddEmtyBuffer(packet);

                return
                    ListArrayEmpty();
            }

            return 
                AddBufferAndGetListOldPackets(packet);
        }

        private IReadOnlyList<UdpPacket> AddBufferAndGetListOldPackets(UdpPacket packet)
        {
            int position =
                (int)(packet.PacketId - _firstId);

            if (CanInsertWithoutShifting(position))
                return AddBufferFitsInWindow(position, packet);
            else
                return AddBufferWithWindowShift(position, packet);
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

        private IReadOnlyList<UdpPacket> AddBufferFitsInWindow(int position, UdpPacket packet)
        {
            if (ContainsId(packet.PacketId))
                return ListArrayEmpty();

            var oldPacket = IsFull ? this[0] : null;

            int idx = InternalIndex(position);
            _buffer[idx] = packet;

            if (IsFull)
                AdvanceWindow();
            else if (position == Capacity && !IsFull)
                EnsureBufferFullState();
            else if (position == Count || !IsFull)
                RecalculateWindowState(position);

            return oldPacket is null 
                ? ListArrayEmpty()
                : new[] { oldPacket };
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

        private IReadOnlyList<UdpPacket> AddBufferWithWindowShift(int position, UdpPacket packet)
        {
            int newLen = position + 1;
            //var oldPackets = new UdpPacket[newLen]; временно  

            //if (NeedsWindowShift(newLen)) временно 
            var oldPackets = ShiftWindowAndEvictOldest(newLen);

            Append(packet);

            return oldPackets;
        }

        private bool NeedsWindowShift(int newLen) =>
            newLen > Capacity;

        private UdpPacket[] ShiftWindowAndEvictOldest(int count)
        {
            int shift = count - Capacity;

            //var oldPackets = CreateEmptyUdpArray(count);
            var oldPackets = CreateEmptyUdpArray(shift);

            for (int i = 0; i < shift; i++)
            {
                // Old realization
                //idx = InternalIndex(i); 
                //oldPackets[i] = this[idx];
                //_buffer[idx] = null;

                oldPackets[i] = this[i];
                this[i] = null;
            }

            _start = InternalIndex(shift);
            _firstId += (uint)shift;

            return oldPackets;
        }

        private void Append(UdpPacket packet)
        {
            this[(Capacity - 1)] = packet;

            Count = Capacity;
            _end = InternalIndex(Count);
        }

        private IReadOnlyList<UdpPacket> ListArrayEmpty() =>
            Array.Empty<UdpPacket>();

        private UdpPacket CreateEmptyPacket()
        {
            return new UdpPacket
            (
                0,
                Array.Empty<byte>()
            );
        }

        private UdpPacket[] CreateEmptyUdpArray(int length) => 
            Enumerable.Repeat(EmptyPacket, length).ToArray();
    }
}

