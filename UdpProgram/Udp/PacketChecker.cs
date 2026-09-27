namespace UdpProgram.Udp
{
    internal class PacketChecker
    {
        public event Action<uint> OnGapDetected;
        public event Action<uint> OnGapFilled;

        private readonly object _lock = new();
        private const uint WindowSize = 512;

        private HashSet<uint> _lostPackets;
        private uint _lastPacketId;

        public PacketChecker()
        {
            _lostPackets = new HashSet<uint>();
            _lastPacketId = uint.MaxValue;
        }


        public void Record(uint packetId)
        {
            lock (_lock)
                UpdateState(packetId);
        }

        private void UpdateState(uint packetId)
        {
            if (_lostPackets.Contains(packetId))
            {
                _lostPackets.Remove(packetId);
                OnGapFilled?.Invoke(packetId);
            }

            else if (IsNextExpected(packetId))
            {
                _lastPacketId = packetId;
                TrimOldGaps();
            }

            else if (IsPacketIdBigger(packetId))
            {
                FillMissingRange(packetId);
                _lastPacketId = packetId;
                TrimOldGaps();
            }
        }

        private void FillMissingRange(uint packetId)
        {
            for (uint i = _lastPacketId + 1; i < packetId; i++)
            {
                _lostPackets.Add(i);
                OnGapDetected?.Invoke(i);
            }
        }

        private void TrimOldGaps()
        {
            if (_lastPacketId < WindowSize)
                return;

            uint cutoff = _lastPacketId - WindowSize;
            _lostPackets.RemoveWhere(id => id < cutoff);
        }

        private bool IsNextExpected(uint packetId) =>
            packetId == _lastPacketId + 1;

        private bool IsPacketIdBigger(uint packetId) =>
            packetId > _lastPacketId + 1;
    }
}