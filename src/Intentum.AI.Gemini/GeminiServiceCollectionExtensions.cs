using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.Gemini;

public static class GeminiServiceCollectionExtensions
{
    public static IServiceCollection AddIntentumGemini(
        this IServiceCollection services,
        GeminiOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IIntentEmbeddingProvider, GeminiEmbeddingProvider>();
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, GeminiIntentModel>();

        return services;
    }
}
