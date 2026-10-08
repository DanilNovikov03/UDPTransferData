using UdpProgram.Udp;

namespace UdpProgram.CircularBuffer
{
    public class PacketCircucleBuffer : MyCircularBuffer<UdpPacket>
    {
        // TODO сделать unit тесты а то слишком сложно каждый раз вручную проверять
        private UdpPacket _emptyPacket;

        private uint _firstId;
        private uint _lastId;

        public PacketCircucleBuffer(int capacity) : base(capacity)
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
            if (IsEmpty)
            {
                AddEmtyBuffer(packet);
                return;
            }

            if (IsOldPacket(packet))
                return;

            int position = 
                (int)(packet.PacketId - _firstId);

            if (position <= Count || !IsFull)
            {
                AddBufferFitsInWindow(position, packet);
                return;
            }

            // 4. Прыжок (position > Count)
            int newLen = position + 1;             // новая длина окна

            if (newLen > Capacity)                 // окно не влезает
            {
                int shift = newLen - Capacity;
                _start = InternalIndex(shift);     // физически сдвигаем начало
                _firstId += (uint)shift;           // логически сдвигаем начало
                newLen = Capacity;                 // ужимаем
            }

            // Кладём пакет **в последнюю ячейку нового окна**
            _buffer[InternalIndex(newLen - 1)] = packet;
            _lastId = packet.PacketId;
            Count = newLen;
            _end = InternalIndex(Count);           // _end = следующая свободная
        }

        private void AddEmtyBuffer(UdpPacket packet)
        {
            _buffer[_start] = packet;         
            _firstId = _lastId = packet.PacketId;
            Count = 1;
            _end = InternalIndex(Count);          
        }

        private void AddBufferFitsInWindow(int position, UdpPacket packet)
        {
            int idx = InternalIndex(position);
            _buffer[idx] = packet;

            if (IsFull)
                AdvanceWindow();
            else if (position == Count || !IsFull)
                RecalculateWindowState();
        }
        
        private void AdvanceWindow()
        {
            Increment(ref _end);
            _start = _end;
            _firstId++;
        }

        private void RecalculateWindowState() =>
            _end = InternalIndex(++Count);

        
        private bool IsOldPacket(UdpPacket packet) =>
            packet.PacketId < _firstId;

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
