using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdpProgram.Udp;

namespace CircularBuffer.CircularBuffer
{
    public class PacketCircucleBuffer : MyCircularBuffer<UdpPacket>
    {
        private uint _firstId;
        private uint _lastId;

        public PacketCircucleBuffer(int capacity) : base(capacity)
        {
        }

        public override UdpPacket this[int index] 
        { 
            get => base[index]; 
            set => base[index] = value; 
        }

        public override void Add(UdpPacket packet)
        {
            // 1. Совсем пустое окно (не буфер, а окно)
            if (Count == 0)
            {
                _buffer[_start] = packet;          // _start обычно 0
                _firstId = _lastId = packet.PacketId;
                Count = 1;
                _end = InternalIndex(1);           // следующая свободная
                return;
            }

            // 2. Слишком старый – за левой границей окна
            if (packet.PacketId < _firstId)
                return;

            int position = (int)(packet.PacketId - _firstId);

            // 3. Внутри окна или ровно за концом (position <= Count)
            if (position <= Count)
            {
                int idx = InternalIndex(position); // физическая ячейка
                _buffer[idx] = packet;

                if (position == Count)             // расширяем окно на 1
                {
                    _lastId = packet.PacketId;
                    Count++;
                    _end = InternalIndex(Count);   // двигаем конец
                }
                // если position < Count – дыра/обновление, ничего не двигаем
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
    }
}
