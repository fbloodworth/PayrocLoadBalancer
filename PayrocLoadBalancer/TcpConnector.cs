using PayrocLoadBalancer.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace PayrocLoadBalancer
{
    public class TcpConnector : ITcpConnector
    {
        public async Task<bool> TryConnectAsync(string host, int port, int timeoutMs)
        {
            try
            {
                using var tcp = new TcpClient();
                var connectTask = tcp.ConnectAsync(IPAddress.Parse(host), port);
                var completed = await Task.WhenAny(connectTask, Task.Delay(timeoutMs));
                return (completed == connectTask && tcp.Connected);
            }
            catch
            {
                return false;
            }
        }
    }
}
