using PayrocLoadBalancer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
