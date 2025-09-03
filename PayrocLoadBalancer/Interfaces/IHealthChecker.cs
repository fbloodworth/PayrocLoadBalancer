namespace PayrocLoadBalancer.Interfaces
{
    public interface IHealthChecker
    {
        Task RunAsync(CancellationToken token);
    }
}
