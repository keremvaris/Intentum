namespace Intentum.Runtime.RateLimiting;

/// <summary>
/// Result of a rate limit check.
/// </summary>
public sealed record RateLimitResult(
    bool Allowed,
    int CurrentCount,
    int Limit,
    TimeSpan? RetryAfter = null
);

/// <summary>
/// Interface for rate limiting. Used when policy decision is <see cref="Policy.PolicyDecision.RateLimit"/>.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Tries to acquire a slot for the given key within the limit for the time window.
    /// </summary>
    /// <param name="key">Scope key (e.g. user id, session id, IP).</param>
    /// <param name="limit">Maximum number of requests allowed in the window.</param>
    /// <param name="window">Time window (e.g. 1 minute).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating whether the request is allowed and optional retry-after.</returns>
    ValueTask<RateLimitResult> TryAcquireAsync(
        string key,
        int limit,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the counter for the given key (e.g. after successful auth or admin override).
    /// </summary>
    void Reset(string key);
}
