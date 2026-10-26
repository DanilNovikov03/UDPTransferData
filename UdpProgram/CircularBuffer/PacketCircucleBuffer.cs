using UdpProgram.Udp;

namespace UdpProgram.CircularBuffer
{
    public class PacketCircucleBuffer : MyCircularBuffer<UdpPacket>
    {
        // TODO возможен прыжок больше размера буффера
        private UdpPacket _emptyPacket;
        private uint _firstId;

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

            if (ContainsId(packet.PacketId)) // temp лучше спрашивать не каждый раз, а когда элемент находится в окне
                return;

            int position = 
                (int)(packet.PacketId - _firstId);

            if (position <= Count || !IsFull)
            {
                AddBufferFitsInWindow(position, packet);
                return;
            }

            // todo вынести в метод
            // 4. Прыжок (position > Count)
            int newLen = position + 1;             // новая длина окна

            if (newLen > Capacity)                 // окно не влезает
            {
                int shift = newLen - Capacity;

                // Затираем старые данные
                for (int i = 0; i < shift; i++)
                    _buffer[InternalIndex(i)] = null;
                
                _start = InternalIndex(shift);     // физически сдвигаем начало
                _firstId += (uint)shift;           // логически сдвигаем начало
                newLen = Capacity;                 // ужимаем
            }

            // Кладём пакет **в последнюю ячейку нового окна**
            _buffer[InternalIndex(newLen - 1)] = packet;
            Count = newLen;
            _end = InternalIndex(Count);           // _end = следующая свободная
        }

        public bool ContainsId(uint packetId) =>
            IndexOfId(packetId) >= 0;

        private void AddEmtyBuffer(UdpPacket packet)
        {
            _buffer[_start] = packet;         
            _firstId = packet.PacketId;
            Count = 1;
            _end = InternalIndex(Count);          
        }

        private void AddBufferFitsInWindow(int position, UdpPacket packet)
        {
            int idx = InternalIndex(position);
            _buffer[idx] = packet;

            if (IsFull)
                AdvanceWindow();
            else if (position == Capacity && !IsFull) // todo переработать, вынести в метод
            {
                Increment(ref _start);
                _end = _start;
                Count = Capacity;
                _firstId++;
            }
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
        private bool IsOldPacket(UdpPacket packet) =>
            packet.PacketId < _firstId;

        // todo можно оптимизировать - сделать вычисление позиции элемента, т.к. знаем все id
        private int IndexOfId(uint packetId)
        {
            for (int i = 0; i < Capacity; i++)
            {
                var p = _buffer[InternalIndex(i)];
                if (p != null && p.PacketId == packetId)
                    return i;
            }
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
