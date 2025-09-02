using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LoadBalancer.Models;

namespace LoadBalancer.Core
{
    public class TcpLoadBalancer
    {
        private readonly TcpListener _listener;
        private readonly BackendServicePool _backendServicePool;

        public TcpLoadBalancer(IPAddress listenAddress, int listenPort, IEnumerable<BackendService> services)
        {
            _listener = new TcpListener(listenAddress, listenPort);
            _backendServicePool = new BackendServicePool(services);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine($"[LB] Listening on {_listener.LocalEndpoint}");

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($"[LB] Client connected: {client.Client.RemoteEndPoint}");
                _ = HandleClientAsync(client); // fire & forget
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var backend = _backendServicePool.GetNextBackend();
            if (backend == null)
            {
                Console.WriteLine("[LB] No healthy backends available. Closing client.");
                client.Close();
                return;
            }

            try
            {
                using var backendClient = new TcpClient();
                await backendClient.ConnectAsync(IPAddress.Parse(backend.Host), backend.Port);

                backend.IncrementConnections();

                using var clientStream = client.GetStream();
                using var backendStream = backendClient.GetStream();

                var t1 = clientStream.CopyToAsync(backendStream);
                var t2 = backendStream.CopyToAsync(clientStream);

                await Task.WhenAny(t1, t2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LB] Error with backend {backend}: {ex.Message}");
                backend.MarkState(ServiceState.Down);
            }
            finally
            {
                backend.DecrementConnections();
                client.Close();
            }
        }
    }
}