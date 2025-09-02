using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrocLoadBalancer
{
    public class LoadBalancerConfig
    {
        public List<BackendConfig> Backends { get; set; }
    }
}
