using UdpProgram.Abstraction.Transport;
using UdpProgram.Protocol;
using UdpProgram.Udp;

namespace UdpProgram.UDPServer
{
    // пока данный класс не обрабатывает сами данные. Потом будет добавлен циклический буффер.
    public class Server
    {
        private ITransport _transportReceive;
        private PacketChecker _packetChecker;

        public Server(ITransport transportReceive)
        {
            _transportReceive = transportReceive;
            _packetChecker = new PacketChecker();
        }


        public async Task StartReceivingAsync()
        {
            while (true)
            {
                Console.WriteLine($"Начало приёма"); // TODO убрать после отладки
                UdpPacket packet = await ReceivePacket();
                //_packetChecker.AddPacketId(packet.PacketId);

                Console.WriteLine($"Пришел пакет {packet.PacketId}"); // TODO убрать после отладки
            }
        }

        private async Task<UdpPacket> ReceivePacket()
        {
            byte[] receivedData = await _transportReceive.ReceiveAsync();
            UdpPacket packet = UdpDataConverter.FromBytesToPacket(receivedData);

            return packet;
        }
    }
}