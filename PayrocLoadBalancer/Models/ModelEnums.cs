namespace LoadBalancer.Models
{
    /// <summary>
    /// Represents the state of a backend service in the load balancer.
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// The service is online and accepting traffic.
        /// </summary>
        Up,

        /// <summary>
        /// The service is offline or unreachable.
        /// </summary>
        Down,

        /// <summary>
        /// The service is being drained (existing connections allowed, new ones blocked).
        /// </summary>
        Draining
    }
}
