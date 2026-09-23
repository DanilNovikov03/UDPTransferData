using System.Collections.Concurrent;

namespace UdpProgram.Udp
{
    internal class PacketChecker
    {
        private int PacketTimeoutMilliseconds;

        private ConcurrentDictionary<uint, DateTime> _expectedPackets;
        private uint _nextExpectedPacketId;
        private List<uint> _lostPackets;
        private Timer _timer;


        public PacketChecker(int packetTimeoutMilliseconds = 50)
        {
            PacketTimeoutMilliseconds = packetTimeoutMilliseconds;

            _nextExpectedPacketId = 1;
            _expectedPackets = new ConcurrentDictionary<uint, DateTime>();
            _lostPackets = new List<uint>();

            _timer = new Timer(CheckForLostPackets, null, PacketTimeoutMilliseconds, PacketTimeoutMilliseconds);
        }


        // TODO сделать второй список ожидаемых пакетов
        public List<uint> GetLostPackets()
        {
            List<uint> copyLostPackets = new List<uint>(_lostPackets);
            //List<uint> copyLostPackets = _lostPackets.ToList(); TODO протестировать
            _lostPackets.Clear();
            return copyLostPackets;
        }

        public void AddPacketId(uint packetId)
        {
            RemovePacketFromWaiting(packetId);
            ProcessIncomingPacketId(packetId);
        }

        private void CheckForLostPackets(object state)
        {
            DateTime now = DateTime.UtcNow;
            foreach (var kvp in _expectedPackets.ToList())
            {
                if (CheckExistenceWaitExpectPacket(now, kvp.Value))
                {
                    RemovePacketFromWaiting(kvp.Key);
                    _lostPackets.Add(kvp.Key);
                }
            }
        }

        private void RemovePacketFromWaiting(uint packetId) =>
            _expectedPackets.TryRemove(packetId, out _);

        private void ProcessIncomingPacketId(uint packetId)
        {
            if (IsNextExpected(packetId))
                _nextExpectedPacketId++;
            else if (IsPacketIdBigger(packetId))
                HandlerPacketIdBigger(packetId);
        }

        private bool IsNextExpected(uint packetId) =>
            packetId == _nextExpectedPacketId;

        private bool IsPacketIdBigger(uint packetId) =>
            packetId > _nextExpectedPacketId;

        private void HandlerPacketIdBigger(uint packetId)
        {
            RecordExpectedPackets(packetId);
            _nextExpectedPacketId = packetId + 1;
        }

        private void RecordExpectedPackets(uint packetId)
        {
            for (uint i = _nextExpectedPacketId; i < packetId; i++)
                _expectedPackets[i] = DateTime.UtcNow;
        }

        private bool CheckExistenceWaitExpectPacket(DateTime now, DateTime packetDateTime) =>
            (now - packetDateTime).TotalMilliseconds > PacketTimeoutMilliseconds;
    }
}