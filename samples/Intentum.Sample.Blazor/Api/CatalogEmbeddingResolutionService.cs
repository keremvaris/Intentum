using Intentum.AI.Catalog;
using Intentum.AI.Embeddings;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Resolves catalog reference embeddings at startup so CatalogIntentModel returns real intent names.
/// </summary>
internal sealed class CatalogEmbeddingResolutionService(
    IIntentEmbeddingProvider embeddingProvider,
    IntentCatalog catalog) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await catalog.ResolveEmbeddingsAsync(embeddingProvider, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
