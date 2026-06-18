using Intentum.AI.DeepSeek;
using Intentum.AI.Embeddings;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI;

public static class DeepSeekServiceCollectionExtensions
{
    public static IServiceCollection AddIntentumDeepSeek(
        this IServiceCollection services,
        DeepSeekOptions options)
    {
        options.Validate();
        services.AddSingleton(options);
        var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/") };
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        services.AddSingleton(httpClient);
        services.AddSingleton<IIntentEmbeddingProvider>(sp =>
            new DeepSeekEmbeddingProvider(sp.GetRequiredService<DeepSeekOptions>(), sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, DeepSeekIntentModel>();

        return services;
    }
}
