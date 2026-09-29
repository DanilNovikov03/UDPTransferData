using System.Net;
using System.Net.Sockets;


public class UDPClient
{
    private Socket sender;
    private Socket receiver;
    private readonly IPAddress sendAddress;
    private readonly IPAddress reciveAddress;
    private readonly int remotePort;

    // статистика
    private int sentPackets = 0; // id пакета
    private int lostPacketsCount = 0; // счетчик потерянных пакетов
    private readonly int packetSize = 1024; // размер пакета
    private readonly int maxLength = 4; // в байтах

    private readonly Dictionary<int, byte[]> sentPacketBuffer = new Dictionary<int, byte[]>();

    public UDPClient(string ipAddress, string ipAddressRecive, int port)
    {
        sendAddress = IPAddress.Parse(ipAddress);
        reciveAddress = IPAddress.Parse(ipAddressRecive);
        remotePort = port;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public async Task StartSendingAsync()
    {
        Console.WriteLine("Отправка пакетов...");

        for (int i = 0; i < 1000000; i++)
        //while (true)
        {
            byte[] data = GeneratePacket();
            await sender.SendToAsync(data, SocketFlags.None, new IPEndPoint(sendAddress, remotePort));
            sentPacketBuffer[sentPackets] = data; // Сохраняем отправленный пакет в буфер
            sentPackets++;
        }
    }


    public async Task ReceiveLostPacketNotificationAsync()
    {
        byte[] data = new byte[4]; // буфер для получения номера потерянного пакета

        receiver.Bind(new IPEndPoint(reciveAddress, remotePort + 1));

        while (true)
        {
            var result = await receiver.ReceiveFromAsync(data, SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
            int lostPacketNumber = BitConverter.ToInt32(data, 0); // извлекаем номер потерянного пакета  

            lostPacketsCount++; //!!!!!!!!!!!!!!!
            // отправляем потерянный пакет повторно
            if (sentPacketBuffer.TryGetValue(lostPacketNumber, out byte[] lostPacket))
            {
                //await sender.SendToAsync(lostPacket, SocketFlags.None, new IPEndPoint(sendAddress, remotePort));
            }
        }
    }

    public async Task DisplayStatisticsAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"Пакеты отправлены: {sentPackets}");
            Console.WriteLine($"Потерянные пакеты (с повторной отправкой): {lostPacketsCount}");
            await Task.Delay(5000);
        }
    }

    private byte[] GeneratePacket()
    {
        byte[] data = new byte[packetSize];
        byte[] packetNumber = BitConverter.GetBytes(sentPackets); // порядковый номер пакета

        if (packetNumber.Length > maxLength)
            sentPackets = 0;

        new Random().NextBytes(data); // генерируем случайные данные
        Buffer.BlockCopy(packetNumber, 0, data, 0, packetNumber.Length);

        return data;
    }
}