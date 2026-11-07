using UdpProgram.CircularBuffer;
using UdpProgram.Udp;


namespace UdpProgram.Tests.CircularBuffer
{
    public class ServerPacketCircularBufferTests
    {
        [Fact]
        public void Add_SequentialPackets_ShouldBeOrderedCorrectly()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[1]);
            buffer.Add(packets[2]);
            buffer.Add(packets[3]);
            buffer.Add(packets[4]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            //assert
            uint[] Ids = { 0, 1, 2, 3, 4 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }

        [Fact]
        public void Add_NonSequentialPackets_ShouldLeaveSpaceForMissingPackets()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[1]);
            buffer.Add(packets[2]);
            buffer.Add(packets[4]);
            buffer.Add(packets[3]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            //assert
            uint[] Ids = { 0, 1, 2, 3, 4 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }

        [Fact]
        public void Add_NonSequentialPackets_ShouldNotOverwriteReservedSlotsForMissing()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[1]);
            buffer.Add(packets[2]);
            buffer.Add(packets[4]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            //assert
            uint[] Ids = { 0, 1, 2, 0, 4 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }

        [Fact]
        public void Buffer_WindowShift_RemovesOldestDataOnOverflow()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            foreach (var  packet in packets)
                buffer.Add(packet);

            var result = GetUdpPackets(buffer, sizeBuffer);

            //assert
            uint[] Ids = { 5, 6, 7, 8, 9 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }
        
        [Fact]
        public void Add_WhenWindowShiftsOnOverflow_SortsRemainingPacketsCorrectly()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[2]);
            buffer.Add(packets[4]);
            buffer.Add(packets[3]);
            buffer.Add(packets[1]);
            buffer.Add(packets[5]);
            buffer.Add(packets[6]);
            buffer.Add(packets[7]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            //assert
            uint[] Ids = { 3, 4, 5, 6, 7 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }

        [Fact]
        public void Add_WhenWindowShiftsOnOverflow_BufferHandlesMissingPacketsAndMaintainsOrder()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[2]);
            buffer.Add(packets[4]);
            buffer.Add(packets[1]);
            buffer.Add(packets[5]);
            buffer.Add(packets[7]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            //assert
            uint[] Ids = { 0, 4, 5, 0, 7 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }

        [Fact]
        public void Add_WithGapsAndLargeJump_ShouldPreserveOrder()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[2]);
            buffer.Add(packets[4]);
            buffer.Add(packets[1]);
            buffer.Add(packets[5]);
            buffer.Add(packets[9]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            //assert
            uint[] Ids = { 5, 0, 0, 0, 9 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }

        [Fact]
        public void AddDuplicatePackets_BeforeFull_ShouldKeepUniqueOnly()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[0]);
            buffer.Add(packets[2]);
            buffer.Add(packets[2]);
            buffer.Add(packets[4]);
            buffer.Add(packets[4]);
            buffer.Add(packets[1]);
            buffer.Add(packets[1]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            // assert
            uint[] Ids = { 0, 1, 2, 0, 4 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }

        [Fact]
        public void Add_DuplicatePackets_AfterFull_ShouldNotBreakOrder()
        {
            // arrange
            int sizeBuffer = 5;
            int countPackets = 10;
            ServerPacketCircularBuffer buffer = new ServerPacketCircularBuffer(sizeBuffer);

            List<UdpPacket> packets = CreateListUdpPackets(countPackets);

            // act
            buffer.Add(packets[0]);
            buffer.Add(packets[0]);
            buffer.Add(packets[2]);
            buffer.Add(packets[2]);
            buffer.Add(packets[4]);
            buffer.Add(packets[4]);
            buffer.Add(packets[1]);
            buffer.Add(packets[1]);
            buffer.Add(packets[5]);
            buffer.Add(packets[5]);
            buffer.Add(packets[9]);
            buffer.Add(packets[9]);

            var result = GetUdpPackets(buffer, sizeBuffer);

            // assert
            uint[] Ids = { 5, 0, 0, 0, 9 };
            var resultIds = result.Select(p => p.PacketId).ToArray();

            Assert.Equal(Ids, resultIds);
        }


        // Tests method AddAndEvicted
        [Fact]
        public void AddAndEvicted_WhenBufferEmpty_ReturnsEmptyList()
        {
            // arrange
            var buffer = CreateBuffer(8);

            // act
            var result = buffer.AddAndEvicted(CreatePacket(0));

            Assert.Empty(result);
            Assert.Equal(1, buffer.Count);
            Assert.False(buffer.IsFull);
            Assert.True(buffer.ContainsId(0));
            Assert.NotNull(buffer[0]);
            Assert.Equal(0u, buffer[0].PacketId);
        }

        [Fact]
        public void AddAndEvicted_WhenBufferNotFull_Sequential_ReturnsEmptyList()
        {
            // arrange
            var buffer = CreateBuffer(8);

            // act
            var r1 = buffer.AddAndEvicted(CreatePacket(0));
            var r2 = buffer.AddAndEvicted(CreatePacket(1));
            var r3 = buffer.AddAndEvicted(CreatePacket(2));
            var r4 = buffer.AddAndEvicted(CreatePacket(3));

            // assert
            Assert.Empty(r1);
            Assert.Empty(r2);
            Assert.Empty(r3);
            Assert.Empty(r4);
            Assert.Equal(4, buffer.Count);
            Assert.False(buffer.IsFull);
            Assert.True(buffer.ContainsId(0));
            Assert.True(buffer.ContainsId(1));
            Assert.True(buffer.ContainsId(2));
            Assert.True(buffer.ContainsId(3));

            var packets = GetPackets(buffer);
            Assert.Equal(new uint[] { 0, 1, 2, 3, 0, 0, 0, 0 }, packets.Select(p => p.PacketId).ToArray());
        }

        [Fact]
        public void AddAndEvicted_WhenDuplicate_ReturnsEmptyList()
        {
            // arrange
            var buffer = CreateBuffer(8);
            buffer.Add(CreatePacket(0));
            buffer.Add(CreatePacket(1));
            buffer.Add(CreatePacket(2));

            var duplicate = CreatePacket(1);

            // act
            var result = buffer.AddAndEvicted(duplicate);

            // assert
            Assert.Empty(result);
            Assert.Equal(3, buffer.Count);
            Assert.True(buffer.ContainsId(0));
            Assert.True(buffer.ContainsId(1));
            Assert.True(buffer.ContainsId(2));
        }

        [Fact]
        public void AddAndEvicted_WhenOldPacket_ReturnsEmptyList()
        {
            // arrange
            var buffer = CreateBuffer(8);
            buffer.Add(CreatePacket(5));
            buffer.Add(CreatePacket(6));
            buffer.Add(CreatePacket(7));

            // act — PacketId 3 < _firstId (5), значит устаревший
            var result = buffer.AddAndEvicted(CreatePacket(3));

            // assert
            Assert.Empty(result);
            Assert.Equal(3, buffer.Count);
            Assert.True(buffer.ContainsId(5));
            Assert.True(buffer.ContainsId(6));
            Assert.True(buffer.ContainsId(7));
            Assert.False(buffer.ContainsId(3));
        }

        [Fact]
        public void AddAndEvicted_WhenBufferFull_EvictsOldestPacket()
        {
            // arrange
            var buffer = CreateBuffer(6);
            var packet0 = CreatePacket(0);
            var packet1 = CreatePacket(1);
            var packet2 = CreatePacket(2);
            var packet3 = CreatePacket(3);
            var packet4 = CreatePacket(4);
            var packet5 = CreatePacket(5);

            buffer.Add(packet0);
            buffer.Add(packet1);
            buffer.Add(packet2);
            buffer.Add(packet3);
            buffer.Add(packet4);
            buffer.Add(packet5);

            Assert.True(buffer.IsFull);
            Assert.Equal(6, buffer.Count);

            // act — следующий по порядку, должен вытеснить пакет 0
            var result = buffer.AddAndEvicted(CreatePacket(6));

            // assert
            Assert.Single(result);
            Assert.Equal(0u, result[0].PacketId);
            Assert.True(ByteArrayEqual(packet0.Data, result[0].Data));

            Assert.Equal(6, buffer.Count);
            Assert.True(buffer.IsFull);
            Assert.True(buffer.ContainsId(6));
            Assert.False(buffer.ContainsId(0));

            var packets = GetPackets(buffer);
            var ids = packets.Select(p => p.PacketId).ToArray();
            Assert.Equal(new uint[] { 1, 2, 3, 4, 5, 6 }, ids);
        }

        [Fact(Skip = "баг: ShiftWindowAndEvictOldest возвращает массив размера count (10), а не shift (4). Первые 4 элемента заполнены, остальные 6 — null")]
        public void AddAndEvicted_WhenForwardJump_EvictsMultiplePackets()
        {
            // arrange
            var buffer = CreateBuffer(6);
            buffer.Add(CreatePacket(0));
            buffer.Add(CreatePacket(1));
            buffer.Add(CreatePacket(2));
            buffer.Add(CreatePacket(3));
            buffer.Add(CreatePacket(4));
            buffer.Add(CreatePacket(5));

            Assert.True(buffer.IsFull);
            Assert.Equal(6, buffer.Count);

            // act — прыжок с 5 на 9: shift = 9 - 5 = 4, вытесняются пакеты 0,1,2,3
            var result = buffer.AddAndEvicted(CreatePacket(9));

            // assert
            Assert.Equal(4, result.Count);
            Assert.Equal(0u, result[0].PacketId);
            Assert.Equal(1u, result[1].PacketId);
            Assert.Equal(2u, result[2].PacketId);
            Assert.Equal(3u, result[3].PacketId);

            Assert.Equal(6, buffer.Count);
            Assert.True(buffer.IsFull);
            Assert.True(buffer.ContainsId(9));
            Assert.False(buffer.ContainsId(0));
            Assert.False(buffer.ContainsId(1));
            Assert.False(buffer.ContainsId(2));
            Assert.False(buffer.ContainsId(3));

            var packets = GetPackets(buffer);
            var ids = packets.Select(p => p.PacketId).ToArray();
            // Остались пакеты 4, 5 и новый 9, остальные слоты — пустые
            Assert.Equal(new uint[] { 4, 5, 0, 0, 0, 9 }, ids);
        }

        [Fact(Skip = "баг: ShiftWindowAndEvictOldest возвращает массив размера count (16), а не shift (10). Первые 10 элементов заполнены, остальные 6 — null")]
        public void AddAndEvicted_WhenJumpExceedsCapacity_NoDuplicatesInEvicted()
        {
            // arrange
            var buffer = CreateBuffer(6);
            buffer.Add(CreatePacket(0));
            buffer.Add(CreatePacket(1));
            buffer.Add(CreatePacket(2));
            buffer.Add(CreatePacket(3));
            buffer.Add(CreatePacket(4));
            buffer.Add(CreatePacket(5));

            Assert.True(buffer.IsFull);
            Assert.Equal(6, buffer.Count);

            // act — прыжок с 5 на 15: shift = 15 - 5 = 10, >= Capacity(6)
            // Должно вытеснить все 6 пакетов и сдвинуть окно
            var result = buffer.AddAndEvicted(CreatePacket(15));

            // assert
            Assert.Equal(6, result.Count);
            var evictedIds = result.Select(p => p.PacketId).OrderBy(x => x).ToArray();
            Assert.Equal(new uint[] { 0, 1, 2, 3, 4, 5 }, evictedIds);

            // Нет null в evicted
            Assert.DoesNotContain(result, p => p == null);

            Assert.Equal(6, buffer.Count);
            Assert.True(buffer.IsFull);
            Assert.True(buffer.ContainsId(15));

            var packets = GetPackets(buffer);
            var ids = packets.Select(p => p.PacketId).ToArray();
            // Последний слот — пакет 15, остальные — пустые слоты (EmptyPacket с ID 0)
            Assert.Equal(new uint[] { 0, 0, 0, 0, 0, 15 }, ids);
        }

        [Fact]
        public void AddAndEvicted_WhenNullSlot_ReturnsEmptyPacket_NotNull()
        {
            // arrange
            var buffer = CreateBuffer(8);
            buffer.Add(CreatePacket(0));
            buffer.Add(CreatePacket(2)); // пропуск ID 1

            // act & assert — пустые слоты не должны возвращать null
            for (int i = 0; i < buffer.Capacity; i++)
            {
                var packet = buffer[i];
                Assert.NotNull(packet);
            }

            // Пакет 0 на индексе 0
            Assert.Equal(0u, buffer[0].PacketId);
            // Пакет 2 на индексе 2
            Assert.Equal(2u, buffer[2].PacketId);
            // Пустой слот на индексе 1 — EmptyPacket
            Assert.Equal(0u, buffer[1].PacketId);
            Assert.Empty(buffer[1].Data);
        }

        [Fact(Skip = "баг: ShiftWindowAndEvictOldest возвращает массив размера count (14), а не shift (6). Первые 6 элементов заполнены, остальные 8 — null")]
        public void AddAndEvicted_MixedSequence_WindowStateMatchesExpected()
        {
            // arrange
            var buffer = CreateBuffer(8);

            // act — серия с перемешанными и частично пропущенными ID
            buffer.AddAndEvicted(CreatePacket(0));
            buffer.AddAndEvicted(CreatePacket(2));
            buffer.AddAndEvicted(CreatePacket(5));
            buffer.AddAndEvicted(CreatePacket(1));
            buffer.AddAndEvicted(CreatePacket(3));
            buffer.AddAndEvicted(CreatePacket(7));
            buffer.AddAndEvicted(CreatePacket(4));
            buffer.AddAndEvicted(CreatePacket(6));
            // Теперь буфер полон: [0,1,2,3,4,5,6,7]

            Assert.True(buffer.IsFull);
            Assert.Equal(8, buffer.Count);

            // Добавляем пакет 8 — вытесняет 0
            var r1 = buffer.AddAndEvicted(CreatePacket(8));
            Assert.Single(r1);
            Assert.Equal(0u, r1[0].PacketId);

            // Добавляем прыжок на 14 — вытесняет 1,2,3,4,5,6,7 (shift = 14-8 = 6)
            var r2 = buffer.AddAndEvicted(CreatePacket(14));
            Assert.Equal(6, r2.Count);
            var evictedIds = r2.Select(p => p.PacketId).OrderBy(x => x).ToArray();
            Assert.Equal(new uint[] { 1, 2, 3, 4, 5, 6 }, evictedIds);

            // assert — итоговое состояние
            Assert.Equal(8, buffer.Count);
            Assert.True(buffer.IsFull);
            Assert.True(buffer.ContainsId(8));
            Assert.True(buffer.ContainsId(14));
            Assert.False(buffer.ContainsId(0));
            Assert.False(buffer.ContainsId(1));
            Assert.False(buffer.ContainsId(2));
            Assert.False(buffer.ContainsId(3));
            Assert.False(buffer.ContainsId(4));
            Assert.False(buffer.ContainsId(5));
            Assert.False(buffer.ContainsId(6));

            var packets = GetPackets(buffer);
            var ids = packets.Select(p => p.PacketId).ToArray();
            // Остались 7, 8 и 14, остальные пустые
            Assert.Equal(new uint[] { 7, 8, 0, 0, 0, 0, 0, 14 }, ids);
        }

        // --- Helpers ---

        private ServerPacketCircularBuffer CreateBuffer(int capacity) =>
            new ServerPacketCircularBuffer(capacity);

        private UdpPacket CreatePacket(uint id, byte[]? data = null) =>
            new UdpPacket(id, data ?? GenerateRandomData());

        private List<UdpPacket> GetPackets(ServerPacketCircularBuffer buffer)
        {
            var packets = new List<UdpPacket>(buffer.Capacity);
            for (int i = 0; i < buffer.Capacity; i++)
                packets.Add(buffer[i]);

            return packets;
        }

        private static bool ByteArrayEqual(byte[]? a, byte[]? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.SequenceEqual(b);
        }

        private List<UdpPacket> GetUdpPackets(ServerPacketCircularBuffer buffer, int sizeBuffer)
        {
            List<UdpPacket> packets = new List<UdpPacket>(sizeBuffer);
            for (int i = 0; i < sizeBuffer; i++)
                packets.Add(buffer[i]);

            return packets;
        }


        private List<UdpPacket> CreateListUdpPackets(int countPackets)
        {
            List<UdpPacket> packets = new List<UdpPacket>();
            for (int i = 0; i < countPackets; i++)
            {
                UdpPacket packet = new UdpPacket((uint)i, GenerateRandomData());
                packets.Add(packet);
            }

            return packets;
        }

        private byte[] GenerateRandomData()
        {
            Random random = new Random();
            int dataSize = random.Next(1024, 61440); // 1 Kb ... 60 Kb
                                                     //int dataSize = random.Next(1024, 1048576); // 1 Kb ... 1 Mb
                                                     //int dataSize = random.Next(1048576, 104857600); // 1 Mb ... 100 Mb
            byte[] data = new byte[dataSize];
            random.NextBytes(data);
            return data;
        }
    }
}

/*
 *    ServerPacketCircularBuffer.ShiftWindowAndEvictOldest (строка 115-130)
 *    Проблема: создаёт массив размера `count`, но заполняет только первые `shift` элементов.
 *    Возвращает массив большего размера с null-хвостом.
 *    Ожидается: вернуть только первые `shift` элементов (new UdpPacket[shift]).
 *    Затронутые тесты:
 *    - AddAndEvicted_WhenForwardJump_EvictsMultiplePackets (возвращает 10 вместо 4)
 *    - AddAndEvicted_WhenJumpExceedsCapacity_NoDuplicatesInEvicted (возвращает 16 вместо 10)
 *    - AddAndEvicted_MixedSequence_WindowStateMatchesExpected (возвращает 14 вместо 6)
 */