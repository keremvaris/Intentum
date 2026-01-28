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
        options.Validate();
        services.AddSingleton(options);
        var httpClient = new HttpClient { BaseAddress = new Uri(options.Endpoint) };
        httpClient.DefaultRequestHeaders.Add("api-key", options.ApiKey);
        services.AddSingleton(httpClient);
        services.AddSingleton<IIntentEmbeddingProvider>(sp =>
            new AzureOpenAIEmbeddingProvider(options, sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, AzureOpenAIIntentModel>();

        return services;
    }
}
