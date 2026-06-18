namespace Intentum.Distributed.RateLimiting;

public interface IDistributedRateLimiter
{
    Task<bool> TryAcquireAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
}
