using UdpProgram.Abstraction.Transport;
using System.Net.Sockets;


namespace UdpProgram.Transport
{
    internal class UdpTransport : ITransport
    {
        private readonly UdpClient _client;

        public UdpTransport(UdpClient client) =>
            _client = client;


        public async Task<byte[]> ReceiveAsync()
        {
            var data = await _client.ReceiveAsync();

            int size = data.Buffer.Length;
            byte[] result = new byte[data.Buffer.Length];
            Array.Copy(data.Buffer, result, data.Buffer.Length);

            return result;
        }

        public async Task SendAsync(byte[] datagram) =>
            await _client.SendAsync(datagram, datagram.Length);
    }
}
