namespace Intentum.Distributed.Locking;

public interface IDistributedLock
{
    Task<bool> AcquireAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default);
    Task ReleaseAsync(string key, CancellationToken cancellationToken = default);
}
