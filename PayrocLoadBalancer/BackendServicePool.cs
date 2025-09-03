using PayrocLoadBalancer.Models;

namespace PayrocLoadBalancer
{
    public class BackendServicePool
    {
        private readonly List<BackendService> _services = [];
        private int _currentIndex = -1;
        private readonly object _lock = new();

        public BackendServicePool(IEnumerable<BackendService> services)
        {
            ArgumentNullException.ThrowIfNull(services);
            _services = [.. services];
        }

        /// <summary>
        /// Returns the next available backend service using round-robin selection.
        /// Skips services that are not in the "Up" state.
        /// </summary>
        public BackendService? GetNextBackend()
        {
            lock (_lock)
            {
                //skip services that are not up
                var availableServices = _services.Where(s => s.IsHealthy == true).ToList();
                if (availableServices.Count == 0)
                {
                    return null;
                }

                _currentIndex = (_currentIndex + 1) % availableServices.Count;
                return availableServices[_currentIndex];
            }
        }

        /// <summary>
        /// Get a snapshot of all services.
        /// </summary>
        public IReadOnlyList<BackendService> GetAllServices()
        {
            lock (_lock)
            {
                return [.. _services];
            }
        }
    }
}
