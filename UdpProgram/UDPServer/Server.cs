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

        private ITransport _transportSend;
        private ServerPacketLossHandler _handlerLoss;

        // TODO Make it more convenient with transportSend so that it doesn’t need to be passed separately to the constructor
        public Server(ITransport transportReceive, ITransport transportSend)
        {
            _transportReceive = transportReceive;
            _packetChecker = new PacketChecker();
            _transportSend = transportSend;

            _handlerLoss = new ServerPacketLossHandler(_transportSend, _packetChecker);
        }


        public async Task StartReceivingAsync()
        {
            Console.WriteLine($"Начало приёма"); // TODO убрать после отладки
            _handlerLoss.Start();
            while (true)
            {
                UdpPacket packet = await ReceivePacket();
                _packetChecker.Record(packet.PacketId);

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