using PayrocLoadBalancer;
using PayrocLoadBalancer.Interfaces;
using PayrocLoadBalancer.Models;

namespace PayrocLoadBalancerTests
{
    public class TcpHealthCheckerTests
    {
        private class FakeTcpConnector : ITcpConnector
        {
            private readonly bool _shouldSucceed;
            public FakeTcpConnector(bool shouldSucceed) => _shouldSucceed = shouldSucceed;

            public Task<bool> TryConnectAsync(string host, int port, int timeoutMs)
            {
                return Task.FromResult(_shouldSucceed);
            }
        }

        [Fact]
        public async Task HealthChecker_ShouldMarkUp_WhenConnectorSucceeds()
        {
            var backend = new BackendService("127.0.0.1", 9001) { };
            backend.MarkState(ServiceState.Down);

            var pool = new BackendServicePool(new List<BackendService> { backend });
            var checker = new TcpHealthChecker(pool, new FakeTcpConnector(true), checkIntervalMs: 5000, timeoutMs: 5000);

            using var cts = new CancellationTokenSource();
            var task = checker.RunAsync(cts.Token);

            // Wait enough time for at least one pass
            await Task.Delay(100);

            cts.Cancel();

            try { await task; } catch (TaskCanceledException) { }

            Assert.Equal(ServiceState.Up, backend.State);
        }

        [Fact]
        public async Task HealthChecker_ShouldMarkDraining_WhenConnectorFails()
        {
            var backend = new BackendService("127.0.0.1", 9001);
            var pool = new BackendServicePool(new List<BackendService> { backend });
            var checker = new TcpHealthChecker(pool, new FakeTcpConnector(false), checkIntervalMs: 5000, timeoutMs: 5000);

            using var cts = new CancellationTokenSource();
            var task = checker.RunAsync(cts.Token);

            // Wait enough time for at least one pass
            await Task.Delay(100);

            cts.Cancel();

            try { await task; } catch (TaskCanceledException) { }

            Assert.Equal(ServiceState.Draining, backend.State);
        }
    }
}
