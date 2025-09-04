using Microsoft.Extensions.Logging;
using PayrocLoadBalancer.Interfaces;
using PayrocLoadBalancer.Models;

namespace PayrocLoadBalancer
{
    public class TcpHealthChecker : IHealthChecker
    {
        private readonly BackendServicePool _pool;
        private readonly int _checkIntervalMs;
        private readonly int _timeoutMs;
        private readonly ITcpConnector _connector;
        private readonly ILogger<TcpHealthChecker> _logger;


        public TcpHealthChecker(BackendServicePool pool, ITcpConnector connector, ILogger<TcpHealthChecker> logger, int checkIntervalMs = 5000, int timeoutMs = 5000)
        {
            _pool = pool;
            _connector = connector;
            _checkIntervalMs = checkIntervalMs;
            _timeoutMs = timeoutMs;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            { 
                    foreach (var backend in _pool.GetAllServices())
                    {
                        try
                        {
                            var isHealthy = await _connector.TryConnectAsync(backend.Host, backend.Port, _timeoutMs);

                            if (isHealthy)
                            {
                                backend.MarkState(Models.ServiceState.Up);
                            }
                            else
                            {
                                HandleUnhealthyBackend(backend);
                            }
                        }
                        catch
                        {
                            HandleUnhealthyBackend(backend);
                        }
                    }
                    
                    await Task.Delay(_checkIntervalMs, token);
            }
        }

        private void HandleUnhealthyBackend(BackendService backend)
        {
            if(backend.State == ServiceState.Up)
            {
                //First time failure, start draining
                backend.MarkDraining();
                _logger.LogInformation($"[HC] Backend {backend.Host} {backend.Port} is draining");
            } 
            else if(backend.State == ServiceState.Draining && backend.ActiveConnections == 0)
            {
                //Safe to remove from rotation
                backend.MarkState(ServiceState.Down);
                _logger.LogInformation($"[HC] Backend {backend.Host} {backend.Port} is down");
            }
        }
    }
}
