using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdpProgram.UDPServer;

namespace UdpProgram.Udp
{
    public class UDPServer
    {
        private Socket receiver;
        PacketChecker packetChecker;
        ServerPacketLossHandler packetLossHandler;
        private readonly IPAddress localAddress;
        private readonly IPAddress remoteAddress;
        private readonly int localPort;

        private const string PacketCountId = "PACKET_COUNT";
        private const string LostPacketsId = "LOST_PACKETS";
        private const string ConfirmationId = "CONFIRMATION";

        private List<byte[]> receivedPackets;
        private FileAssembler fileAssembler;
        private uint expectedPackets;

        public UDPServer(string localIpAddress, int localPort, string remoteIpAddress)
        {
            localAddress = IPAddress.Parse(localIpAddress);
            this.localPort = localPort;
            remoteAddress = IPAddress.Parse(remoteIpAddress);
            expectedPackets = 0;
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiver.ReceiveBufferSize = 65535; //!!!!!!!!!!!!
            receiver.ReceiveTimeout = 2000; // !!!!!!!!!!!!!
            receivedPackets = new List<byte[]>();
            packetChecker = new PacketChecker();
            packetLossHandler = new ServerPacketLossHandler(packetChecker, this);
            fileAssembler = new FileAssembler();
        }

        public async Task StartReceivingAsync()
        {
            Console.WriteLine("Начало приема пакетов...");

            receiver.Bind(new IPEndPoint(localAddress, localPort));
            byte[] buffer = new byte[65535];

            while (true)
            {
                var result = await receiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
                byte[] receivedData = buffer.Take(result.ReceivedBytes).ToArray();
                DataProcessing(receivedData);
            }
        }

        public void SendLostPackets(List<uint> lostPacketIds)
        {
            string lostPacketsMessage = $"{LostPacketsId}:{string.Join(",", lostPacketIds)}";
            byte[] lostPacketsData = Encoding.UTF8.GetBytes(lostPacketsMessage);
            EndPoint clientEndPoint = new IPEndPoint(remoteAddress, localPort + 1);
            receiver.SendTo(lostPacketsData, clientEndPoint);
            Console.WriteLine("Список потерянных пакетов отправлен клиенту.");
        }

        private void DataProcessing(byte[] receivedData)
        {
            if (IsPacketCountMessage(receivedData))
            {
                expectedPackets = BitConverter.ToUInt32(receivedData, PacketCountId.Length);
                packetChecker.SetExpectedPacketCount(expectedPackets);
                packetLossHandler.Start(expectedPackets);

                Console.WriteLine($"Ожидается {expectedPackets} пакетов.");
            }
            else
            {
                UdpPacket packet = UdpPacket.FromBytes(receivedData);

                if (!packetChecker.AddReceivedPacketId(packet.PacketId))
                {
                    Console.WriteLine($"Получен неожиданный пакет с ID {packet.PacketId}. Пакет проигнорирован.");
                    return;
                }

                //packetChecker.AddReceivedPacketId(packet.PacketId);

                receivedPackets.Add(packet.Data);
                fileAssembler.AddPacket(packet.PacketId, packet.Data);
                Console.WriteLine($"Получен пакет {packet.PacketId} размером {packet.ToBytes().Length} байт");

                if (packetChecker.HaveAllPackages())
                    DataProcessingReceived();
            }
        }

        private bool IsPacketCountMessage(byte[] data)
        {
            string id = Encoding.UTF8.GetString(data, 0, PacketCountId.Length);
            return id == PacketCountId;
        }

        private void SendConfirmation()
        {
            string confirmationMessage = $"{ConfirmationId}";
            byte[] confirmationData = Encoding.UTF8.GetBytes(confirmationMessage);
            EndPoint confirmationEndPoint = new IPEndPoint(remoteAddress, localPort + 1);
            receiver.SendTo(confirmationData, confirmationEndPoint);
            Console.WriteLine("Подтверждение отправлено клиенту.");
        }

        private void DataProcessingReceived()
        {
            Console.WriteLine("Все пакеты получены");
            fileAssembler.AssembleFile();
            SendConfirmation();
            expectedPackets = 0;
            packetChecker.Reset();
            receivedPackets.Clear();
            fileAssembler.Reset();
            packetLossHandler.Stop();
        }
    }
}