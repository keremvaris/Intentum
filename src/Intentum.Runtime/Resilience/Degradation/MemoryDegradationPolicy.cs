namespace Intentum.Runtime.Resilience.Degradation;

public sealed class MemoryDegradationPolicy : IDegradationPolicy
{
    private readonly DegradationOptions _options;
    private readonly object _lock = new();
    private int _consecutiveFailures;
    private DateTime? _degradedSince;

    public MemoryDegradationPolicy(DegradationOptions options)
    {
        _options = options;
    }

    public bool IsDegraded
    {
        get
        {
            CheckRecovery();
            return _consecutiveFailures >= _options.DegradationThreshold;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Func<T> degradedFallback)
    {
        if (IsDegraded)
            return degradedFallback();

        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch
        {
            OnFailure();
            throw;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _consecutiveFailures = 0;
            _degradedSince = null;
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
            _consecutiveFailures = 0;
    }

    private void OnFailure()
    {
        lock (_lock)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _options.DegradationThreshold)
                _degradedSince ??= DateTime.UtcNow;
        }
    }

    private void CheckRecovery()
    {
        if (_degradedSince.HasValue &&
            DateTime.UtcNow - _degradedSince.Value >= _options.CheckInterval)
        {
            Reset();
        }
    }
}
