using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.OpenAI;

public static class OpenAIServiceCollectionExtensions
{
    public static IServiceCollection AddIntentumOpenAI(
        this IServiceCollection services,
        OpenAIOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IIntentEmbeddingProvider, OpenAIEmbeddingProvider>();
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, OpenAIIntentModel>();

        return services;
    }
}
