namespace PayrocLoadBalancer.Interfaces
{
    public interface ITcpConnector
    {
        Task<bool> TryConnectAsync(string host, int port, int timeoutMs);
    }
}
