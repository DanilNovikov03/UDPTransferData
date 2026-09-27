using UdpProgram.Abstraction.Transport;
using UdpProgram.Protocol;


namespace UdpProgram.UDPClient
{
    internal class ClientPacketLossHandler
    {
        private ITransport _transport;

        public ClientPacketLossHandler(ITransport transport)
        {
            _transport = transport;
            StartListening();
        }


        // TODO Think about how to do it in a separate thread
        private void StartListening()
        {
            Task.Run(async () =>
            {
                {
                    byte[] receivedData = await _transport.ReceiveAsync();
                    HandleReceivedMessage(receivedData);
                }
            });
        }

        private void HandleReceivedMessage(byte[] data)
        {
            string message = UdpDataConverter.BytesToString(data);

            if (message.StartsWith(UdpProtocolConstant.LostPacketsId))
                HandlerLostPacket(message);
        }

        private void HandlerLostPacket(string message)
        {
            string lostPacketsMessage = UdpDataConverter.RemovePrefix(message, UdpProtocolConstant.LostPacketsId);
            List<uint> lostPackets = UdpDataConverter.ParseLostIPackets(lostPacketsMessage);

            Console.WriteLine("Получен список потерянных пакетов: " + string.Join(", ", lostPackets)); // TODO Убрать после отладки
        }
    }
}
