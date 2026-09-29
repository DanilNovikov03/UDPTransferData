namespace UdpProgram.Abstraction.Transport
{
    public interface ITransport
    {
        public Task<byte[]> ReceiveAsync();
        public Task SendAsync(byte[] datagram);
    }
}
