using UdpProgram.Abstraction.Transport;
using UdpProgram.Protocol;


namespace UdpProgram.UDPClient
{
    internal class ClientPacketLossHandler
    {
        // TODO create an interface for the circular buffer
        // TODO implement resending
        private readonly ITransport _transport;
        private readonly CancellationTokenSource _cts = new();
        private Task? _listenTask;

        public ClientPacketLossHandler(ITransport transportReceive) =>
            _transport = transportReceive;


        public void Start()
        {
            if (_listenTask != null) return;
            _listenTask = ListenAsync(_cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
            try { _listenTask?.Wait(); } catch { /* Ignored */ }
        }


        private async Task ListenAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested) // test
            {
                try
                {
                    byte[] receivedData = await _transport.ReceiveAsync();
                    HandleReceivedMessage(receivedData);
                }
                catch (OperationCanceledException)
                    { break; }
                catch (ObjectDisposedException)
                    { break; }
                catch (Exception ex)
                    { Console.WriteLine($"Ошибка ClientHandler : {ex.Message}"); }
            }
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
