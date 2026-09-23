using System.Net.Sockets;
using System.Net;
using System.Text;
using UdpProgram.Udp;
using UdpProgram.Protocol;
using UdpProgram.UDPClient;

public class UDPClient
{
    public List<UdpPacket> SentPackets { get; private set; }

    private Socket _sender;
    private Socket _confirmationReceiver;
    private readonly IPAddress _serverAddress;
    private readonly IPAddress _confirmationAddress;
    private readonly int _serverPort;
    private readonly int _confirmationPort;
    private ClientPacketLossHandler _packetLossHandler;

    // Constant
    private const string ConfirmationId = "CONFIRMATION";
    private const string LostPacketsId = "LOST_PACKETS";

    public UDPClient(string serverIpAddress, int serverPort, string confirmationIpAddress, int confirmationPort)
    {
        SentPackets = new List<UdpPacket>();
        _packetLossHandler = new ClientPacketLossHandler(this);

        _serverAddress = IPAddress.Parse(serverIpAddress);
        this._serverPort = serverPort;
        _sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _sender.ReceiveTimeout = 1000; // !!!!!!!!!!!
        _sender.SendTimeout = 1000; //!!!!!!!!!!!

        _confirmationAddress = IPAddress.Parse(confirmationIpAddress);
        this._confirmationPort = confirmationPort;
        _confirmationReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _confirmationReceiver.Bind(new IPEndPoint(_confirmationAddress, this._confirmationPort));
    }

    public async Task StartSendingAsync(byte[] data) =>
        await PacketSendingAsync(data);

    public async Task SendPacketAsync(UdpPacket packet)
    {
        byte[] packetBytes = packet.ToBytes();
        await SendDataToAsync(packetBytes);

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
        byte[] idBytes = UdpDataConverter.StringToBytes(UdpProtocolConstant.PacketCountId);
        byte[] totalPacketsBytes = BitConverter.GetBytes(totalPackets);
        byte[] message = new byte[idBytes.Length + totalPacketsBytes.Length];

        Buffer.BlockCopy(idBytes, 0, message, 0, idBytes.Length);
        Buffer.BlockCopy(totalPacketsBytes, 0, message, idBytes.Length, totalPacketsBytes.Length);

        await SendDataToAsync(message);
    }

    private async Task SendDataToAsync(byte[] data)
    {
        var arrayData = new ArraySegment<byte>(data);
        var ipPoint = new IPEndPoint(_serverAddress, _serverPort);

        await _sender.SendToAsync(arrayData, SocketFlags.None, ipPoint);
    }

    private async Task WaitForConfirmationOrResendAsync()
    {
        bool allPacketsConfirmed = false;

        while (!allPacketsConfirmed)
        {
            byte[] buffer = new byte[65535];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            var result = await _confirmationReceiver.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, remoteEndPoint);
            string message = Encoding.UTF8.GetString(buffer, 0, result.ReceivedBytes);

            if (message.StartsWith(UdpProtocolConstant.ConfirmationId))
            {
                Console.WriteLine("Подтверждение получено от сервера: " + message);
                SentPackets.Clear();
                allPacketsConfirmed = true;
            }
            else if (message.StartsWith(UdpProtocolConstant.LostPacketsId))
            {
                string[] parts = message.Substring(LostPacketsId.Length + 1).Split(',');
                List<uint> lostPackets = parts.Select(uint.Parse).ToList();

                Console.WriteLine("Потерянные пакеты: " + string.Join(", ", lostPackets));
                _packetLossHandler.SetLostPacketIds(lostPackets);
                await _packetLossHandler.ResendLostPacketsAsync();
            }
        }
        if (!allPacketsConfirmed)
            Console.WriteLine("Не удалось получить подтверждение от сервера после нескольких попыток.");
    }
}