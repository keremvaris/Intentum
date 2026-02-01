using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Core.Caching;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.Caching;

/// <summary>
/// Wraps an intent model and caches results by behavior vector hash (exact match).
/// Use to skip recomputation when the same or identical vector was inferred recently.
/// For similarity-threshold matching (e.g. "close enough" vector), implement a custom cache key strategy.
/// </summary>
public sealed class CachedIntentModel : IIntentModel
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IIntentModel _inner;
    private readonly IIntentResultCache _cache;
    private readonly TimeSpan? _ttl;

    /// <summary>
    /// Creates a cached intent model.
    /// </summary>
    /// <param name="inner">The inner intent model.</param>
    /// <param name="cache">Cache for intent results (e.g. MemoryIntentResultCache or Redis-backed).</param>
    /// <param name="ttl">Optional time-to-live for cached entries.</param>
    public CachedIntentModel(IIntentModel inner, IIntentResultCache cache, TimeSpan? ttl = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _ttl = ttl;
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var key = VectorToCacheKey(vector);
        var (found, value) = _cache.TryGetAsync(key).GetAwaiter().GetResult();
        if (found && value != null)
        {
            try
            {
                return JsonSerializer.Deserialize<Intent>(value, JsonOptions)!;
            }
            catch
            {
                // Invalid cache entry; fall through to infer
            }
        }

        var intent = _inner.Infer(behaviorSpace, vector);
        var json = JsonSerializer.Serialize(intent, JsonOptions);
        _cache.SetAsync(key, json, _ttl).GetAwaiter().GetResult();
        return intent;
    }

    private static string VectorToCacheKey(BehaviorVector vector)
    {
        var sb = new StringBuilder();
        foreach (var kv in vector.Dimensions.OrderBy(k => k.Key, StringComparer.Ordinal))
            sb.Append(kv.Key).Append(':').Append(kv.Value).Append(';');
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes);
    }
}
