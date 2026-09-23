
using UdpProgram.Udp;
/*
await Task.Run(async () =>
{
    string serverAddress = "192.168.56.101";
    int port = 4004;

    string confirmAddresIp = "192.168.56.1";
    int portConfirm = port + 1;

    UDPClient client = new UDPClient(serverAddress, port, confirmAddresIp, portConfirm);

    while (true)
    {
        byte[] data = GenerateRandomData();
        await client.StartSendingAsync(data);
        //await Task.Delay(100); // Добавляем задержку перед следующей передачей данных
    }
});

static byte[] GenerateRandomData()
{
    Random random = new Random();
    int dataSize = random.Next(1024, 1048576); // 1 Kb ... 1 Mb
    //int dataSize = random.Next(1048576, 104857600); // 1 Mb ... 100 Mb
    byte[] data = new byte[dataSize];
    random.NextBytes(data);
    return data;
}
*/
await Task.Run(async () =>
{
    string localIp = "192.168.1.103";
    string remoteIp = "192.168.1.104";
    int port = 4004;

    UDPServer server = new UDPServer(localIp, port, remoteIp);

    await server.StartReceivingAsync();
});