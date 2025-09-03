namespace PayrocLoadBalancer.Models
{
    /// <summary>
    /// Represents a single backend service (server) in the load balancer pool.
    /// </summary>
    public class BackendService
    {
        /// <summary>
        /// The host or IP address of the backend service.
        /// </summary>
        public string Host { get; init; }

        /// <summary>
        /// The port number the backend service listens on.
        /// </summary>
        public int Port { get; init; }

        /// <summary>
        /// Current health state of the backend service.
        /// </summary>
        public ServiceState State { get; private set; } = ServiceState.Up;

        /// <summary>
        /// Number of active client connections currently routed to this backend.
        /// </summary>
        public int ActiveConnections { get; private set; } = 0;

        /// <summary>
        /// Timestamp of the last health check.
        /// </summary>
        public DateTime LastCheck { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Returns true if the backend is available for new traffic.
        /// </summary>
        public bool IsHealthy => State == ServiceState.Up;

        public BackendService(string host, int port)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
        }

        public void MarkState(ServiceState newState)
        {
            State = newState;
            LastCheck = DateTime.UtcNow;
        }

        public void IncrementConnections() => ActiveConnections++;
        public void DecrementConnections() => ActiveConnections = Math.Max(0,
        ActiveConnections - 1);

        public void MarkDraining()
        {
            if(State == ServiceState.Up)
            {
                State = ServiceState.Draining;
                LastCheck = DateTime.UtcNow;
            }
        }

        public bool IsRemoveable => State == ServiceState.Draining && ActiveConnections == 0;
    }
}

