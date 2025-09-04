using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayrocLoadBalancer.Interfaces;
using System.Net;

namespace PayrocLoadBalancer
{
    public class LoadBalancerService : BackgroundService
    {
        private readonly ILogger<LoadBalancerService> _logger;
        private readonly TcpLoadBalancer _loadBalancer;

        public LoadBalancerService(ILogger<LoadBalancerService> logger, BackendServicePool pool, ITcpConnector connector)
        {
            _logger = logger;
            _loadBalancer = new TcpLoadBalancer(IPAddress.Any, 8000, pool.GetAllServices(), connector);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting TCP load balancer...");
            await _loadBalancer.StartAsync();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping load balancer...");
            _loadBalancer.Stop();
            return base.StopAsync(cancellationToken);
        }
    }
}
