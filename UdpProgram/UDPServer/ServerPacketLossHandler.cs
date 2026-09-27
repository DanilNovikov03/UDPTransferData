using UdpProgram.Abstraction.Transport;
using UdpProgram.Protocol;
using UdpProgram.Udp;

namespace UdpProgram.UDPServer
{
    internal class ServerPacketLossHandler
    {
        // TODO Create an interface for interacting with the circular buffer.
        private ITransport _transport;
        private PacketChecker _packetChecker;
        private Timer _timer;

        public ServerPacketLossHandler(ITransport transport, PacketChecker checker, int timeSendLostPacketIds = 100)
        {
            _transport = transport;
            _packetChecker = checker;

            _timer = new Timer(SendLostIdPackets, null, timeSendLostPacketIds, timeSendLostPacketIds);
        }

        private async void SendLostIdPackets(object state)
        {
            List<uint> lostPackets = _packetChecker.GetIdLostPackets();
            if (lostPackets.Any())
            {
                byte[] data = MessageLostIdPackets(lostPackets);

                string message = string.Join(", ", lostPackets);
                Console.WriteLine("Отправлен список потерянных пакетов: " + message); // TODO убрать после отладки

                await _transport.SendAsync(data);
            }
        }

        private byte[] MessageLostIdPackets(List<uint> lostPackets)
        {
            string lostPacketsMessage = string.Join(", ", lostPackets);
            string message = UdpProtocolConstant.LostPacketsId + lostPacketsMessage;
            byte[] data = UdpDataConverter.StringToBytes(message);

            return data;
        }
    }
}