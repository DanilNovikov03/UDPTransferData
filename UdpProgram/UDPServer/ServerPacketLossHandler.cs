using System.Collections.Concurrent;
using UdpProgram.Abstraction.Transport;
using UdpProgram.Protocol;
using UdpProgram.Udp;

namespace UdpProgram.UDPServer
{
    internal class ServerPacketLossHandler
    {
        // TODO Make sure the sockets are closed properly.
        // TODO create a counter for repeated requests
        private ITransport _transport;

        private ConcurrentDictionary<uint, DateTime> _expectedPackets;
        private int _timeSendLostPacketIds;

        private PacketChecker _packetChecker;

        private Timer _timer;

        public ServerPacketLossHandler(ITransport transportSend, PacketChecker packetChecker, int timeSendLostPacketIds = 100)
        {
            _transport = transportSend;

            _expectedPackets = new ConcurrentDictionary<uint, DateTime>();
            _timeSendLostPacketIds = timeSendLostPacketIds;

            _packetChecker = packetChecker;
            _packetChecker.OnGapDetected += HandleGapDetected;
            _packetChecker.OnGapFilled += HandleGapFilled;
        }

        public void Start()
        {
            if (_timer != null) return;
            _timer = new Timer(SendLostIdPackets, null, _timeSendLostPacketIds, _timeSendLostPacketIds);
        }

        public void Stop() =>
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);

        private void HandleGapDetected(uint packetId) =>
            _expectedPackets.TryAdd(packetId, DateTime.UtcNow);

        private void HandleGapFilled(uint packetId) =>
            _expectedPackets.TryRemove(packetId, out _);

        private async void SendLostIdPackets(object state)
        {
            try
            {
                List<uint> lostPackets = GetListLostPackets();
                if (lostPackets.Any())
                {
                    byte[] data = MessageLostIdPackets(lostPackets);

                    string message = string.Join(", ", lostPackets);
                    Console.WriteLine("Отправлен список потерянных пакетов: " + message); // TODO убрать после отладки

                    await _transport.SendAsync(data);
                }
            }
            catch (Exception ex)
                { Console.WriteLine($"Ошибка отправки потерянных пакетов: {ex.Message}"); }
        }

        private List<uint> GetListLostPackets()
        {
            List<uint> lostPackets = new();
            DateTime now = DateTime.UtcNow;
            foreach (var kvp in _expectedPackets)
            {
                if (CheckExistenceWaitExpectPacket(now, kvp.Value))
                {
                    _expectedPackets.TryRemove(kvp.Key, out _);
                    lostPackets.Add(kvp.Key);
                }
            }

            return lostPackets;
        }


        private bool CheckExistenceWaitExpectPacket(DateTime now, DateTime packetDateTime) =>
            (now - packetDateTime).TotalMilliseconds > _timeSendLostPacketIds;

        private byte[] MessageLostIdPackets(List<uint> lostPackets)
        {
            string lostPacketsMessage = string.Join(", ", lostPackets);
            string message = UdpProtocolConstant.LostPacketsId + lostPacketsMessage;
            byte[] data = UdpDataConverter.StringToBytes(message);

            return data;
        }
    }
}