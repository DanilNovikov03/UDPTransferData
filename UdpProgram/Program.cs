using System.Net;
using System.Net.Sockets;
using UdpProgram.Abstraction.Transport;
using UdpProgram.Transport;
using UdpProgram.Udp;
using UdpProgram.UDPClient;
using UdpProgram.UDPServer;

await Task.Run(async () =>
{
    string localIp = "192.168.1.100";
    string remoteIp = "192.168.1.103";
    int port = 4004;

    var localEndpoint = new IPEndPoint(IPAddress.Parse(remoteIp), port);

    using UdpClient udp = new UdpClient(remoteIp, port);
    ITransport transport = new UdpTransport(udp);

    Client client = new Client(transport);

    while (true)
    {
        byte[] data = GenerateRandomData();
        List<UdpPacket> packets = UdpSeparationData.SplitIntoPackets(data);
        await client.SendPacketAsync(packets[0]);
        await Task.Delay(1000); // Добавляем задержку перед следующей передачей данных
    }
});

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
await Task.Run(async () =>
{
    string localIp = "192.168.1.100";
    string remoteIp = "192.168.1.103";
    int portReceive = 4004;
    var localEndpoint = new IPEndPoint(IPAddress.Parse(localIp), portReceive);

    UdpClient udp = new UdpClient(portReceive);
    ITransport transport = new UdpTransport(udp);

    Server server = new Server(transport);

    await server.StartReceivingAsync();
});
*/