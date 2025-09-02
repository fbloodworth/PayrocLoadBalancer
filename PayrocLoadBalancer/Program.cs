using LoadBalancer.Core;
using PayrocLoadBalancer;

class Program
{
    static async Task Main(string[] args)
    {
        var backends = ConfigLoader.LoadServices("config.json");

        var balancer = new TcpLoadBalancer(System.Net.IPAddress.Any, 8000, backends);
        await balancer.StartAsync();
    }
        
}
