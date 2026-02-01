using Intentum.AI.Embeddings;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Intentum.AspNetCore.HealthChecks;

/// <summary>
/// Health check for embedding providers.
/// </summary>
public sealed class EmbeddingProviderHealthCheck : IHealthCheck
{
    private readonly IIntentEmbeddingProvider _embeddingProvider;

    public EmbeddingProviderHealthCheck(IIntentEmbeddingProvider embeddingProvider)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var testKey = "health:check";
            var embedding = _embeddingProvider.Embed(testKey);
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Embedding provider is healthy. Test embedding score: {embedding.Score:F2}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Embedding provider health check failed",
                ex));
        }
    }
}
