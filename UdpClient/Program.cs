//using UdpProgram.Udp;


await Task.Run(SendingMessagesAsync);

async Task SendingMessagesAsync()
{
    //string ipAddress = "127.0.0.1";
    string sendAddress = "192.168.56.101";
    string reciveAddress = "192.168.56.1";
    int port = 4004;

    UDPClient udpClient = new UDPClient(sendAddress, reciveAddress, port);

    // запускаем отправку сообщений и прослушку потерянных пактов
    await Task.WhenAll(
        udpClient.DisplayStatisticsAsync(),
        udpClient.StartSendingAsync()
        //udpClient.ReceiveLostPacketNotificationAsync()
    );
}