using System;
namespace PayrocLoadBalancer.Models
{
    public class LoadBalancerConfig
    {
        public List<BackendConfig> Backends { get; set; } = [];
    }
}
