using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdpProgram.Udp;

namespace UdpProgram.UDPClient
{
    public class ClientPacketLossHandler
    {
        private readonly UDPClient udpClient;
        private List<uint> idLostPackets;
        private List<UdpPacket> lostPackets;


        public ClientPacketLossHandler(UDPClient udpClient) 
        {
            this.udpClient = udpClient;
            idLostPackets = new List<uint>();
            lostPackets = new List<UdpPacket>();
        }


        public async Task ResendLostPacketsAsync()
        {
            foreach (var packet in lostPackets)
                await udpClient.SendPacketAsync(packet);
            
            idLostPackets.Clear();
            lostPackets.Clear();
        }

        public void SetLostPacketIds(List<uint> lostPacketIds)
        {
            idLostPackets = lostPacketIds;
            IdentifyLostPackets();
        }

        private void IdentifyLostPackets()
        {
            lostPackets.Clear();

            foreach (var packet in udpClient.SentPackets)
                if (idLostPackets.Contains(packet.PacketId))
                    lostPackets.Add(packet);
        }

    }
}
