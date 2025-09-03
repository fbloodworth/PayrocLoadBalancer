using System;
namespace PayrocLoadBalancer.Models
{
    public class BackendConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
    }
}
