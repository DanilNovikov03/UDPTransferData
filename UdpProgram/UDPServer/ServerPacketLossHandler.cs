using System.Net.Sockets;
using UdpProgram.Protocol;
using UdpProgram.Udp;

namespace UdpProgram.UDPServer
{
    internal class ServerPacketLossHandler
    {
        private UdpClient _sender;
        private PacketChecker _packetChecker;
        private Timer _timer;

        public ServerPacketLossHandler(string clientIpAddres, int clientPort, PacketChecker checker, int timeSendLostPacketIds = 100)
        {
            _sender = new UdpClient(clientIpAddres, clientPort);
            _packetChecker = checker;

            _timer = new Timer(SendLostPacketIds, null, timeSendLostPacketIds, timeSendLostPacketIds);
        }

        private async void SendLostPacketIds(object state)
        {
            List<uint> lostPackets = _packetChecker.GetLostPackets();
            if (lostPackets.Any())
            {
                byte[] data = MessageLostPacketIds(lostPackets);

                string message = string.Join(", ", lostPackets);
                Console.WriteLine("Отправлены потерянные пакеты: " + message); // TODO убрать

                await _sender.SendAsync(data, data.Length);
            }
        }

        private byte[] MessageLostPacketIds(List<uint> lostPackets)
        {
            string lostPacketsMessage = string.Join(", ", lostPackets);
            string message = UdpProtocolConstant.LostPacketsId + lostPacketsMessage;
            byte[] data = UdpDataConverter.StringToBytes(message);

            return data;
        }
    }
}