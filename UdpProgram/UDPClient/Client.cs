using System.Net.Sockets;
using UdpProgram.Protocol;
using UdpProgram.Udp;
using UdpProgram.UDPClient;


public class Client
{
    private UdpClient _sender;
    private ClientPacketLossHandler _lostPacketHandler;

    public Client(string serverIpAddress, int serverPort)
    {
        _sender = new UdpClient(serverIpAddress, serverPort);
        _lostPacketHandler = new ClientPacketLossHandler(serverPort + 1);
    }

    // TODO test, after delete
    public async Task SendAsync(byte[] data)
    {
        var packet = UdpDataConverter.FromBytesToPacket(data);
        await _sender.SendAsync(data, data.Length);
    }

    //  A ready packet arrives in the class from the circular buffer
    public async Task SendPacketAsync(UdpPacket packet)
    {
        byte[] packetBytes = UdpDataConverter.ToBytesPacket(packet);
        await _sender.SendAsync(packetBytes, packetBytes.Length);
    }
}