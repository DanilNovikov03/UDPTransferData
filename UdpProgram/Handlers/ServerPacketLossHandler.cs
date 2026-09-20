using UdpProgram.Udp;

namespace UdpProgram.Handlers
{
    public class ServerPacketLossHandler
    {
        private readonly PacketChecker packetChecker;
        private readonly UDPServer udpServer;
        private readonly Timer packetCheckTimer;
        private uint expectedPackets;
        private int checkIntervalMilliseconds;

        public ServerPacketLossHandler(PacketChecker packetChecker, UDPServer udpServer, int checkIntervalMilliseconds = 100)
        {
            this.packetChecker = packetChecker;
            this.udpServer = udpServer;
            this.checkIntervalMilliseconds = checkIntervalMilliseconds;
            packetCheckTimer = new Timer(CheckForLostPackets, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start(uint expectedPackets)
        {
            this.expectedPackets = expectedPackets;
            packetChecker.SetExpectedPacketCount(expectedPackets);
            packetCheckTimer.Change(checkIntervalMilliseconds, checkIntervalMilliseconds);
        }

        public void Stop() =>
            packetCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);

        private void CheckForLostPackets(object state)
        {
            List<uint> missingIds;
            lock (packetChecker)
            {
                missingIds = packetChecker.FullGetMissingPacketIds();
            }

            if (missingIds.Any())
            {
                lock (packetChecker)
                {
                    packetChecker.AddMissingPackets(missingIds);
                }
                Console.WriteLine("Обнаружены потерянные пакеты: " + string.Join(", ", missingIds));
                udpServer.SendLostPackets(missingIds);
            }
        }
    }
}