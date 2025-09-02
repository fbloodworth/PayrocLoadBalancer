using LoadBalancer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PayrocLoadBalancer
{
    public static class ConfigLoader
    {
        public static List<BackendService> LoadServices(string path)
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<LoadBalancerConfig>(json);

            var services = new List<BackendService>();
            foreach(var backend in config.Backends)
            {
                services.Add(new BackendService(backend.Host, backend.Port));
            }

            return services;
        }
    }
}
