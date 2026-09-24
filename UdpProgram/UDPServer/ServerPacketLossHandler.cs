using System.Net.Sockets;
using UdpProgram.Protocol;
using UdpProgram.Udp;

namespace UdpProgram.UDPServer
{
    internal class ServerPacketLossHandler
    {
        // TODO Create an interface for interacting with the circular buffer.
        private UdpClient _sender;
        private PacketChecker _packetChecker;
        private Timer _timer;

        public ServerPacketLossHandler(string clientIpAddres, int clientPort, PacketChecker checker, int timeSendLostPacketIds = 100)
        {
            _sender = new UdpClient(clientIpAddres, clientPort);
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

                await _sender.SendAsync(data, data.Length);
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