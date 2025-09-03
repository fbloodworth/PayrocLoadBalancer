using PayrocLoadBalancer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
namespace PayrocLoadBalancer
{
    public static class ConfigLoader
    {
        public static List<BackendService> LoadServices(string path)
        {
            var services = new List<BackendService>();

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<LoadBalancerConfig>(json);

            if (config is not null)
            {
                foreach (var backend in config.Backends)
                {
                    services.Add(new BackendService(backend.Host, backend.Port));
                }
            }

            return services;
        }
    }
}
