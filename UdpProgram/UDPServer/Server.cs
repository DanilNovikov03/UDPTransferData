using System.Net.Sockets;
using UdpProgram.Udp;
using UdpProgram.Protocol;

namespace UdpProgram.UDPServer
{
    // пока данный класс не обрабатывает сами данные. Потом будет добавлен циклический буффер.
    public class Server
    {
        private UdpClient _receiver;
        private PacketChecker _packetChecker;
        private ServerPacketLossHandler _lostPacketHandler;

        public Server(string clientIpAddres, int port)
        {
            _receiver = new UdpClient(port);
            _packetChecker = new PacketChecker();
            _lostPacketHandler = new ServerPacketLossHandler(clientIpAddres, port + 1, _packetChecker); // TODO сделать получение ip клиента отдельной процедурой
        }

        public async Task StartReceivingAsync()
        {
            while (true)
            {
                UdpPacket packet = await ReceivePacket();
                _packetChecker.AddPacketId(packet.PacketId);

                Console.WriteLine($"Пришел пакет {packet.PacketId}"); // TODO убрать после отладки
            }
        }

        private async Task<UdpPacket> ReceivePacket()
        {
            var receivedData = await _receiver.ReceiveAsync();
            byte[] receiveBytes = receivedData.Buffer;

            UdpPacket packet = UdpDataConverter.FromBytesToPacket(receiveBytes);

            return packet;
        }
    }
}