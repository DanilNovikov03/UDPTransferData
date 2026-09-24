using UdpProgram.Udp;

namespace UdpProgram.UDPClient
{
    public class ClientPacketLossHandler
    {
        private readonly Client _client;
        private List<uint> _idLostPackets;
        private List<UdpPacket> _lostPackets;


        public ClientPacketLossHandler(Client udpClient) 
        {
            _client = udpClient;
            _idLostPackets = new List<uint>();
            _lostPackets = new List<UdpPacket>();
        }


        public async Task ResendLostPacketsAsync()
        {
            foreach (var packet in _lostPackets)
                await _client.SendPacketAsync(packet);
            
            _idLostPackets.Clear();
            _lostPackets.Clear();
        }

        public void SetLostPacketIds(List<uint> lostPacketIds)
        {
            _idLostPackets = lostPacketIds;
            IdentifyLostPackets();
        }

        private void IdentifyLostPackets()
        {
            _lostPackets.Clear();

            foreach (var packet in _client.SentPackets)
                if (_idLostPackets.Contains(packet.PacketId))
                    _lostPackets.Add(packet);
        }

    }
}
