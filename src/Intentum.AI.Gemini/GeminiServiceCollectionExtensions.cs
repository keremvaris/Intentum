using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.Gemini;

[UsedImplicitly]
public static class GeminiServiceCollectionExtensions
{
    [UsedImplicitly]
    public static IServiceCollection AddIntentumGemini(
        this IServiceCollection services,
        GeminiOptions options)
    {
        options.Validate();
        services.AddSingleton(options);
        services.AddSingleton(new HttpClient { BaseAddress = new Uri(options.BaseUrl!) });
        services.AddSingleton<IIntentEmbeddingProvider>(sp =>
            new GeminiEmbeddingProvider(options, sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, GeminiIntentModel>();

        return services;
    }
}
