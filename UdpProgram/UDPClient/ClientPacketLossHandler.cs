using System.Net.Sockets;
using UdpProgram.Protocol;


namespace UdpProgram.UDPClient
{
    internal class ClientPacketLossHandler
    {
        private UdpClient _receiverCommand;

        public ClientPacketLossHandler(int clientPort)
        {
            _receiverCommand = new UdpClient(clientPort);
            StartListening();
        }


        // TODO Think about how to do it in a separate thread
        private void StartListening()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var receivedData = await _receiverCommand.ReceiveAsync();
                    HandleReceivedMessage(receivedData.Buffer);
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
