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
        options.Validate();
        services.AddSingleton(options);
        var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        services.AddSingleton(httpClient);
        services.AddSingleton<IIntentEmbeddingProvider>(sp =>
            new MistralEmbeddingProvider(options, sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, MistralIntentModel>();

        return services;
    }
}
