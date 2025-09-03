using PayrocLoadBalancer.Interfaces;
using PayrocLoadBalancer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
namespace PayrocLoadBalancer
{
    public class TcpHealthChecker : IHealthChecker
    {
        private readonly BackendServicePool _pool;
        private readonly int _checkIntervalsMs;
        private readonly int _timeouts;

        public TcpHealthChecker(BackendServicePool pool, int checkIntervalsMs = 5000, int timeouts = 500)
        {
            _pool = pool;
            _checkIntervalsMs = checkIntervalsMs;
            _timeouts = timeouts;
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            { 
                    foreach (var backend in _pool.GetAllServices())
                    {
                        try
                        {
                            using var tcpclient = new TcpClient();
                            var connectTask = tcpclient.ConnectAsync(backend.Host, backend.Port);
                            var completed = await Task.WhenAny(connectTask, Task.Delay(_timeouts, token));

                            if (completed == connectTask && tcpclient.Connected)
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
                    
                    await Task.Delay(_checkIntervalsMs, token);
            }
        }

        private void HandleUnhealthyBackend(BackendService backend)
        {
            if(backend.State == ServiceState.Up)
            {
                //First time failure, start draining
                backend.MarkDraining();
                Console.WriteLine($"[HC] Backend {backend.Host} {backend.Port} is draining");
            } 
            else if(backend.State == ServiceState.Draining && backend.ActiveConnections == 0)
            {
                //Safe to remove from rotation
                backend.MarkState(ServiceState.Down);
                Console.WriteLine($"[HC] Backend {backend.Host} {backend.Port} is down");
            }
        }
    }
}
