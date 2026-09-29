using System.Text;
using UdpProgramStatistic;

string ip = "192.168.56.101";

CustomUdpClient client = new CustomUdpClient("192.168.56.101", 4004); // IP-адрес вашей виртуальной машины
while (true)
{
    // Пример отправки данных
    byte[] data = GenerateRandomData();
    await client.StartSendingAsync(data);
    await Task.Delay(100);
}


static byte[] GenerateRandomData()
{
    Random random = new Random();
    //int dataSize = random.Next(1024, 1048576); // 1 Kb ... 1 Mb
    int dataSize = random.Next(1048576, 104857600); // 1 Mb ... 100 Mb
    byte[] data = new byte[dataSize];
    random.NextBytes(data);
    return data;
}


/*
int port = 4004;
CustomUdpServer server = new CustomUdpServer(port, ip);
await server.StartReceivingAsync();
*/