using System.Collections.Concurrent;

namespace Intentum.Runtime.RateLimiting;

/// <summary>
/// In-memory rate limiter using a fixed window per key.
/// Suitable for single-node or development; use a distributed implementation for multi-node.
/// </summary>
public sealed class MemoryRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, Window> _windows = new();

    /// <inheritdoc />
    public ValueTask<RateLimitResult> TryAcquireAsync(
        string key,
        int limit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        var windowKey = $"{key}:{window.Ticks}";
        var current = _windows.AddOrUpdate(
            windowKey,
            _ => new Window(now, window, 1),
            (_, w) => w.Tick(now, limit));

        var allowed = current.Count <= limit;
        TimeSpan? retryAfter = null;
        if (!allowed && current.WindowEnd > now)
            retryAfter = current.WindowEnd - now;

        var result = new RateLimitResult(
            Allowed: allowed,
            CurrentCount: current.Count,
            Limit: limit,
            RetryAfter: retryAfter);

        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public void Reset(string key)
    {
        var toRemove = _windows.Keys.Where(k => k.StartsWith(key + ":", StringComparison.Ordinal)).ToList();
        foreach (var k in toRemove)
            _windows.TryRemove(k, out _);
    }

    private sealed class Window
    {
        public DateTimeOffset WindowStart { get; }
        public DateTimeOffset WindowEnd { get; }
        public TimeSpan Duration { get; }
        public int Count { get; }

        public Window(DateTimeOffset start, TimeSpan duration, int count)
        {
            WindowStart = start;
            Duration = duration;
            WindowEnd = start + duration;
            Count = count;
        }

        public Window Tick(DateTimeOffset now, int limit)
        {
            if (now >= WindowEnd)
                return new Window(now, Duration, 1);
            return new Window(WindowStart, Duration, Count + 1);
        }
    }
}
