using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.AzureOpenAI;

public static class AzureOpenAIServiceCollectionExtensions
{
    public static IServiceCollection AddIntentumAzureOpenAI(
        this IServiceCollection services,
        AzureOpenAIOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IIntentEmbeddingProvider, AzureOpenAIEmbeddingProvider>();
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, AzureOpenAIIntentModel>();

        return services;
    }
}
