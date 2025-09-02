using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadBalancer.Models
{
    public class BackendServicePool
    {
        private readonly List<BackendService> _services = new();
        private int _currentIndex = -1;
        private readonly object _lock = new();

        public BackendServicePool(IEnumerable<BackendService> services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            _services = services.ToList();
        }

        /// <summary>
        /// Returns the next available backend service using round-robin selection.
        /// Skips services that are not in the "Up" state.
        /// </summary>
        public BackendService? GetNextBackend()
        {
            lock (_lock)
            {
                var availableServices = _services.Where(s => s.State == ServiceState.Up).ToList();
                if (availableServices.Count == 0)
                {
                    return null; // No healthy services
                }

                _currentIndex = (_currentIndex + 1) % availableServices.Count;
                return availableServices[_currentIndex];
            }
        }

        /// <summary>
        /// Add a new backend service to the pool.
        /// </summary>
        public void AddService(BackendService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            lock (_lock)
            {
                _services.Add(service);
            }
        }

        /// <summary>
        /// Remove a backend service from the pool.
        /// </summary>
        public void RemoveService(BackendService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            lock (_lock)
            {
                _services.Remove(service);
            }
        }

        /// <summary>
        /// Get a snapshot of all services.
        /// </summary>
        public IReadOnlyList<BackendService> GetAllServices()
        {
            lock (_lock)
            {
                return _services.ToList();
            }
        }
    }
}
