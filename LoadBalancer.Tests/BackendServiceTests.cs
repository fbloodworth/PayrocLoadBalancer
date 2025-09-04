using PayrocLoadBalancer.Models;

namespace PayrocLoadBalancerTests
{
    public class BackendServiceTests
    {
        [Fact]
        public void NewBackend_ShouldStartUp()
        {
            var backend = new BackendService("127.0.0.1", 9001);
            Assert.Equal(ServiceState.Up, backend.State);
            Assert.Equal(0, backend.ActiveConnections);
        }

        [Fact]
        public void IncrementAndDecrementConnections_ShouldWork()
        {
            var backend = new BackendService("127.0.0.1", 9001);

            backend.IncrementConnections();
            Assert.Equal(1, backend.ActiveConnections);

            backend.DecrementConnections();
            Assert.Equal(0, backend.ActiveConnections);
        }

        [Fact]
        public void MarkDraining_ShouldChangeState()
        {
            var backend = new BackendService("127.0.0.1", 9001);

            backend.MarkDraining();
            Assert.Equal(ServiceState.Draining, backend.State);
        }

        [Fact]
        public void DrainingService_WithZeroConnections_ShouldBeRemovable()
        {
            var backend = new BackendService("127.0.0.1", 9001);
            backend.MarkDraining();

            Assert.True(backend.IsRemoveable);
        }
    }
}
