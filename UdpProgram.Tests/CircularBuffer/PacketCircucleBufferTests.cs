using UdpProgram.CircularBuffer;
using UdpProgram.Udp;


namespace UdpProgram.Tests.CircularBuffer
{
    public class PacketCircucleBufferTests
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
