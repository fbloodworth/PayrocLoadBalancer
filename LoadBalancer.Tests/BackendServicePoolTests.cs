using PayrocLoadBalancer;
using PayrocLoadBalancer.Models;

namespace PayrocLoadBalancerTests
{
    public class BackendServicePoolTests
    {
        [Fact]
        public void GetNextBackend_ShouldReturnRoundRobin()
        {
            var services = new List<BackendService>
            {
                new("127.0.0.1", 9001),
                new("127.0.0.1", 9002)
            };
            var pool = new BackendServicePool(services);

            var first = pool.GetNextBackend();
            var second = pool.GetNextBackend();
            var third = pool.GetNextBackend();

            Assert.NotNull(first);
            Assert.Equal("127.0.0.1", first.Host);
            Assert.Equal(9001, first.Port);

            Assert.NotNull(second);
            Assert.Equal("127.0.0.1", second.Host);
            Assert.Equal(9002, second.Port);

            Assert.NotNull(third);
            Assert.Equal("127.0.0.1", third.Host);
            Assert.Equal(9001, third.Port); // back to first
        }

        [Fact]
        public void GetNextBackend_ShouldSkipDraining()
        {
            var s1 = new BackendService("127.0.0.1", 9001);
            var s2 = new BackendService("127.0.0.1", 9002);

            s2.MarkDraining();

            var pool = new BackendServicePool([s1, s2]);

            var backend = pool.GetNextBackend();
            Assert.NotNull(backend);
            Assert.Equal(9001, backend.Port); // skips draining
        }

        [Fact]
        public void GetNextBackend_ShouldReturnNull_WhenAllDown()
        {
            var s1 = new BackendService("127.0.0.1", 9001);
            s1.MarkState(ServiceState.Down);

            var pool = new BackendServicePool(new List<BackendService> { s1 });

            var backend = pool.GetNextBackend();
            Assert.Null(backend);
        }
    }
}

