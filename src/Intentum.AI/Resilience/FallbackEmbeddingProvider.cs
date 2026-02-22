using Intentum.AI.Embeddings;

namespace Intentum.AI.Resilience;

/// <summary>
/// Embedding provider that tries multiple providers in order, falling back to the next
/// when one fails. Implements a simple circuit breaker: after consecutive failures,
/// a provider is skipped for a cooldown period before being retried.
/// </summary>
public sealed class FallbackEmbeddingProvider : IIntentEmbeddingProvider
{
    private readonly IReadOnlyList<IIntentEmbeddingProvider> _providers;
    private readonly int _maxConsecutiveFailures;
    private readonly TimeSpan _cooldownPeriod;
    private readonly int[] _consecutiveFailures;
    private readonly DateTimeOffset[] _lastFailureTimes;

    public FallbackEmbeddingProvider(
        IEnumerable<IIntentEmbeddingProvider> providers,
        int maxConsecutiveFailures = 3,
        TimeSpan? cooldownPeriod = null)
    {
        _providers = (providers ?? throw new ArgumentNullException(nameof(providers))).ToList();
        if (_providers.Count == 0)
            throw new ArgumentException("At least one provider is required.", nameof(providers));

        _maxConsecutiveFailures = maxConsecutiveFailures;
        _cooldownPeriod = cooldownPeriod ?? TimeSpan.FromMinutes(1);
        _consecutiveFailures = new int[_providers.Count];
        _lastFailureTimes = new DateTimeOffset[_providers.Count];
    }

    public IntentEmbedding Embed(string behaviorKey)
        => EmbedAsync(behaviorKey, CancellationToken.None).GetAwaiter().GetResult();

    public async Task<IntentEmbedding> EmbedAsync(string behaviorKey, CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var i = 0; i < _providers.Count; i++)
        {
            if (IsCircuitOpen(i))
                continue;

            try
            {
                var result = await _providers[i].EmbedAsync(behaviorKey, cancellationToken);
                _consecutiveFailures[i] = 0;
                return result;
            }
            catch (Exception ex)
            {
                _consecutiveFailures[i]++;
                _lastFailureTimes[i] = DateTimeOffset.UtcNow;
                lastException = ex;
            }
        }

        throw new InvalidOperationException(
            "All embedding providers failed.", lastException);
    }

    private bool IsCircuitOpen(int index)
    {
        if (_consecutiveFailures[index] < _maxConsecutiveFailures)
            return false;

        var elapsed = DateTimeOffset.UtcNow - _lastFailureTimes[index];
        if (elapsed >= _cooldownPeriod)
        {
            _consecutiveFailures[index] = 0;
            return false;
        }

        return true;
    }
}
