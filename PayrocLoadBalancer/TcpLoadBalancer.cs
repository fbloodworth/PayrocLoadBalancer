using System.Net;
using System.Net.Sockets;
using PayrocLoadBalancer.Interfaces;
using PayrocLoadBalancer.Models;

namespace PayrocLoadBalancer
{
    public class TcpLoadBalancer
    {
        private readonly TcpListener _listener;
        private readonly BackendServicePool _backendServicePool;
        private readonly IHealthChecker _healthChecker;
        private readonly CancellationTokenSource _cts = new();

        public TcpLoadBalancer(IPAddress listenAddress, int listenPort, IEnumerable<BackendService> services, ITcpConnector tcpConnector)
        {
            _listener = new TcpListener(listenAddress, listenPort);
            _backendServicePool = new BackendServicePool(services);
            _healthChecker = new TcpHealthChecker(_backendServicePool, tcpConnector);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine($"[LB] Listening on {_listener.LocalEndpoint}");

            _ = _healthChecker.RunAsync(_cts.Token); //background health checking

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($"[LB] Client connected: {client.Client.RemoteEndPoint}");
                _ = HandleClientAsync(client); // fire & forget
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
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

            TcpClient? backendClient = null;
            NetworkStream? clientStream = null;
            NetworkStream? backendStream = null;

            try
            {
                backendClient = new TcpClient
                {
                    ReceiveTimeout = 5000, // 5s read timeout
                    SendTimeout = 5000, // 5s send timeout
                };

                client.ReceiveTimeout = 5000;
                client.SendTimeout = 5000;

                await backendClient.ConnectAsync(IPAddress.Parse(backend.Host), backend.Port);

                backend.IncrementConnections();

                clientStream = client.GetStream();
                backendStream = backendClient.GetStream();

                //use linked tasks to prevent hangs
                var t1 = clientStream.CopyToAsync(backendStream);
                var t2 = backendStream.CopyToAsync(clientStream);

                await Task.WhenAny(t1, t2);
            }
            catch (IOException ioEx)
            { 
                Console.WriteLine($"[LB] IO Error with backend {backend}: {ioEx.Message}");
                backend.MarkState(ServiceState.Down);
            }
            catch (SocketException sockEx)
            {
                Console.WriteLine($"[LB] Socket Error with backend {backend}: {sockEx.Message}");
                backend.MarkState(ServiceState.Down);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LB] Error with backend {backend}: {ex.Message}");
                backend.MarkState(ServiceState.Down);
            }
            finally
            {
                try
                {
                    backendStream?.Close();
                    clientStream?.Close();
                    backendClient?.Close();
                    client.Close();
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine($"[LB] Cleanup error: {ex.Message}");
                }

                backend.DecrementConnections();

                //If backend is draining and has no more active connections, mark it down
                if (backend.IsRemoveable)
                {
                    backend.MarkState(ServiceState.Down);
                    Console.WriteLine($"[LB] Backend {backend} fully drained and removed from pool.");
                }

            }
        }
    }
}