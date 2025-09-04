using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayrocLoadBalancer;
using PayrocLoadBalancer.Interfaces;

class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            // Load backends
            var backends = ConfigLoader.LoadServices("config.json");
            var pool = new BackendServicePool(backends);
            // Register dependencies
            services.AddSingleton(pool);
            services.AddSingleton<ITcpConnector, TcpConnector>();
            services.AddHostedService<LoadBalancerService>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        })
        .Build();

        await host.RunAsync();
    }
}
