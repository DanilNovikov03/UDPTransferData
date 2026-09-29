using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters;
using UdpProgram.Abstraction.Transport;
using UdpProgram.Transport;
using UdpProgram.Udp;
using UdpProgram.UDPClient;
using UdpProgram.UDPServer;


string localIp = "192.168.1.100";
string remoteIp = "192.168.1.103";
int port = 4004;

var localEndpoint = new IPEndPoint(IPAddress.Parse(remoteIp), port);

using UdpClient udpSend = new UdpClient(remoteIp, port);
using UdpClient udpReceive = new UdpClient(port + 1);
ITransport transportSend = new UdpTransport(udpSend);
ITransport transportReceive = new UdpTransport(udpReceive);

Client client = new Client(transportSend, transportReceive);
client.StartHandlerLost();

for (uint i =  0; i < uint.MaxValue; i++)
{
    byte[] data = GenerateRandomData();
    List<UdpPacket> packets = UdpSeparationData.SplitIntoPackets(data);
    packets[0].PacketId = i; // temporary solution

    await client.SendPacketAsync(packets[0]);
    if (i % 100 == 0)
        await Task.Delay(1000); // Добавляем задержку перед следующей передачей данных
}


static byte[] GenerateRandomData()
{
    Random random = new Random();
    int dataSize = random.Next(1024, 61440); // 1 Kb ... 60 Kb
    //int dataSize = random.Next(1024, 1048576); // 1 Kb ... 1 Mb
    //int dataSize = random.Next(1048576, 104857600); // 1 Mb ... 100 Mb
    byte[] data = new byte[dataSize];
    random.NextBytes(data);
    return data;
}

/*
// TODO Perhaps it would be worth handling the socket closure via IDisposible and "using"

string localIp = "192.168.1.100";
string remoteIp = "192.168.1.103";
int portReceive = 4004;
var localEndpoint = new IPEndPoint(IPAddress.Parse(localIp), portReceive);

using UdpClient udpReceive = new UdpClient(remoteIp, portReceive);
using UdpClient udpSend = new UdpClient(remoteIp, portReceive + 1);
ITransport transportReceive = new UdpTransport(udpReceive);
ITransport transportSend = new UdpTransport(udpSend);

Server server = new Server(transportReceive, transportSend);

await server.StartReceivingAsync();

*/