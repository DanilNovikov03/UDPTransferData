namespace UdpProgram.Udp
{
    public class PacketChecker
    {
        private readonly object lockObject = new object();

        private HashSet<uint> _receivedPacketIds;
        private List<uint> _missingPacketIds; // todo: Perhaps it’s worth rewriting it using HashSet.

        private uint? _expectedPacketCount;
        private uint _maxReceivedPacketId;

        public PacketChecker(uint? expectedPacketCount = null)
        {
            _receivedPacketIds = new HashSet<uint>();
            _missingPacketIds = new List<uint>();
            _expectedPacketCount = expectedPacketCount;
            _maxReceivedPacketId = 0;
        }


        public void SetExpectedPacketCount(uint expectedPacketCount)
        {
            lock (lockObject)
                _expectedPacketCount = expectedPacketCount;
        }

        public void ResetExpectedPacketCount()
        {
            lock (lockObject)
                _expectedPacketCount = null;
        }

        public bool HasLostPackets()
        {
            lock (lockObject)
                return _missingPacketIds.Count > 0;
        }

        public List<uint> GetMissingPacketIds()
        {
            lock (lockObject)
                //return new List<uint>(MissingPacketIds);
                return _missingPacketIds; // todo: Test, If it’s bad, then remove it
        }

        public void AddMissingPackets(IEnumerable<uint> missingPackets)
        {
            lock (lockObject)
            {
                var newMissingPackets = missingPackets.Except(_missingPacketIds).ToList();
                _missingPacketIds.AddRange(newMissingPackets);
            }
        }

        public bool AddReceivedPacketId(uint packetId)
        {
            if (IsExpectedCountMissing())
                return false;

            lock (lockObject)
            {
                _receivedPacketIds.Add(packetId);
                CheckMissingPacketId(packetId);
            }
            return true;
        }

        public bool HaveAllPackages()
        {
            EnsureExpectedCountIsSet();

            lock (lockObject)
                return IsAllPacketsReceived();
        }

        public List<uint> FullGetMissingPacketIds()
        {
            EnsureExpectedCountIsSet();
            lock (lockObject)
                return SearchMissingPackets();
        }

        public void Reset()
        {
            lock (lockObject)
                ResetAllValues();
        }


        private bool IsAllPacketsReceived() =>
            (_receivedPacketIds.Count == _expectedPacketCount) && !HasLostPackets();

        private void CheckMissingPacketId(uint packetId)
        {
            CheckPreviousPackets(packetId);

            _maxReceivedPacketId = packetId > _maxReceivedPacketId ? packetId : _maxReceivedPacketId;
            _missingPacketIds.Remove(packetId);
        }

        private void CheckPreviousPackets(uint packetId)
        {
            for (uint i = _maxReceivedPacketId; i < packetId; i++)
                if (IsKnownPacket(packetId))
                    _missingPacketIds.Add(i);
        }

        private bool IsKnownPacket(uint packetId) =>
            !HasPacketReceived(packetId) && !_missingPacketIds.Contains(packetId);

        private List<uint> SearchMissingPackets()
        {
            List<uint> missingPacketsId = new List<uint>();

            for (uint i = 0; i < _expectedPacketCount; i++)
                if (!HasPacketReceived(i))
                    missingPacketsId.Add(i);

            return missingPacketsId;
        }

        private bool HasPacketReceived(uint packetId) =>
            _receivedPacketIds.Contains(packetId);

        private void ResetAllValues()
        {
            _receivedPacketIds.Clear();
            _missingPacketIds.Clear();
            _expectedPacketCount = null;
            _maxReceivedPacketId = 0;
        }

        private void EnsureExpectedCountIsSet()
        {
            if (IsExpectedCountMissing())
                throw new InvalidOperationException("ExpectedPacketCount is not set.");
        }

        private bool IsExpectedCountMissing() =>
            !_expectedPacketCount.HasValue;
    }
}