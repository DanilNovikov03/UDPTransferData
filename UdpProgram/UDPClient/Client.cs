using UdpProgram.Abstraction.Transport;
using UdpProgram.Protocol;
using UdpProgram.Udp;


namespace UdpProgram.UDPClient
{
    public class Client
    {
        private ITransport _transportSend;
        private ITransport _transportReceive;
        private ClientPacketLossHandler _lostPacketHandler;

        public Client(ITransport transportSend, ITransport transportReceive)
        {
            _transportSend = transportSend;
            _transportReceive = transportReceive;

            _lostPacketHandler = new ClientPacketLossHandler(transportReceive);
        }

        // TODO test, after delete
        public async Task SendAsync(byte[] data)
        {
            var packet = UdpDataConverter.FromBytesToPacket(data);
            await _transportSend.SendAsync(data);
        }

        //  A ready packet arrives in the class from the circular buffer
        public async Task SendPacketAsync(UdpPacket packet)
        {
            byte[] packetBytes = UdpDataConverter.ToBytesPacket(packet);
            //Console.WriteLine($"Отправлен Пакет {packet.PacketId}");
            await _transportSend.SendAsync(packetBytes);
        }

        public void StartHandlerLost() =>
            _lostPacketHandler.Start();
    }
}