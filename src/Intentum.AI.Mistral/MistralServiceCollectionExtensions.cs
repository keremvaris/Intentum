using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.Mistral;

public static class MistralServiceCollectionExtensions
{
    public static IServiceCollection AddIntentumMistral(
        this IServiceCollection services,
        MistralOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IIntentEmbeddingProvider, MistralEmbeddingProvider>();
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, MistralIntentModel>();

        return services;
    }
}
