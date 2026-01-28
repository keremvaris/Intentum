using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.Claude;

public static class ClaudeServiceCollectionExtensions
{
    public static IServiceCollection AddIntentumClaude(
        this IServiceCollection services,
        ClaudeOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IIntentEmbeddingProvider, ClaudeEmbeddingProvider>();
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, ClaudeIntentModel>();

        return services;
    }
}
