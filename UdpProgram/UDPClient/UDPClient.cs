using System.Net.Sockets;
using System.Net;
using System.Text;
using UdpProgram.Udp;
using UdpProgram.Handlers;

public class UDPClient
{
    public List<UdpPacket> SentPackets { get; private set; }

    private Socket sender;
    private Socket confirmationReceiver;
    private readonly IPAddress serverAddress;
    private readonly IPAddress confirmationAddress;
    private readonly int serverPort;
    private readonly int confirmationPort;
    private ClientPacketLossHandler packetLossHandler;

    private const string PacketCountId = "PACKET_COUNT";
    private const string ConfirmationId = "CONFIRMATION";
    private const string LostPacketsId = "LOST_PACKETS";

    public UDPClient(string serverIpAddress, int serverPort, string confirmationIpAddress, int confirmationPort)
    {
        SentPackets = new List<UdpPacket>();
        packetLossHandler = new ClientPacketLossHandler(this);

        serverAddress = IPAddress.Parse(serverIpAddress);
        this.serverPort = serverPort;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sender.ReceiveTimeout = 1000; // !!!!!!!!!!!
        sender.SendTimeout = 1000; //!!!!!!!!!!!

        confirmationAddress = IPAddress.Parse(confirmationIpAddress);
        this.confirmationPort = confirmationPort;
        confirmationReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        confirmationReceiver.Bind(new IPEndPoint(confirmationAddress, this.confirmationPort));
    }

    public async Task StartSendingAsync(byte[] data) =>
        await PacketSendingAsync(data);

    public async Task SendPacketAsync(UdpPacket packet)
    {
        byte[] packetBytes = packet.ToBytes();
        await sender.SendToAsync(new ArraySegment<byte>(packetBytes), SocketFlags.None, new IPEndPoint(serverAddress, serverPort));
        Console.WriteLine($"Отправлен пакет {packet.PacketId} размером в {packet.Data.Length} байт");
    }

    private async Task PacketSendingAsync(byte[] data)
    {
        List<UdpPacket> packets = UdpSeparationData.SplitIntoPackets(data);

        int totalPackets = packets.Count;
        await SendPacketCountAsync(totalPackets);

        Console.WriteLine($"Количество пакетов {totalPackets}");

        foreach (var packet in packets)
        {
            SentPackets.Add(packet);
            await SendPacketAsync(packet);
        }
        await WaitForConfirmationOrResendAsync();
    }

    private async Task SendPacketCountAsync(int totalPackets)
    {
        byte[] idBytes = Encoding.UTF8.GetBytes(PacketCountId);
        byte[] totalPacketsBytes = BitConverter.GetBytes(totalPackets);
        byte[] message = new byte[idBytes.Length + totalPacketsBytes.Length];

        Buffer.BlockCopy(idBytes, 0, message, 0, idBytes.Length);
        Buffer.BlockCopy(totalPacketsBytes, 0, message, idBytes.Length, totalPacketsBytes.Length);

        await sender.SendToAsync(new ArraySegment<byte>(message), SocketFlags.None, new IPEndPoint(serverAddress, serverPort));
    }

    private async Task WaitForConfirmationOrResendAsync()
    {
        bool allPacketsConfirmed = false;

        while (!allPacketsConfirmed)
        {
            byte[] buffer = new byte[65535];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            var result = await confirmationReceiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEndPoint);
            string message = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);

            if (message.StartsWith(ConfirmationId))
            {
                Console.WriteLine("Подтверждение получено от сервера: " + message);
                SentPackets.Clear();
                allPacketsConfirmed = true;
            }
            else if (message.StartsWith(LostPacketsId))
            {
                string[] parts = message.Substring(LostPacketsId.Length + 1).Split(',');
                List<uint> lostPackets = parts.Select(uint.Parse).ToList();

                Console.WriteLine("Потерянные пакеты: " + string.Join(", ", lostPackets));
                packetLossHandler.SetLostPacketIds(lostPackets);
                await packetLossHandler.ResendLostPacketsAsync();
            }
        }
        if (!allPacketsConfirmed)
            Console.WriteLine("Не удалось получить подтверждение от сервера после нескольких попыток.");
    }
}