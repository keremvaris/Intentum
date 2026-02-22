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

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var testKey = "health:check";
            var embedding = await _embeddingProvider.EmbedAsync(testKey, cancellationToken);
            return HealthCheckResult.Healthy(
                $"Embedding provider is healthy. Test embedding score: {embedding.Score:F2}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Embedding provider health check failed",
                ex);
        }
    }
}
