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
        options.Validate();
        services.AddSingleton(options);
        var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        services.AddSingleton(httpClient);
        services.AddSingleton<IIntentEmbeddingProvider>(sp =>
            new OpenAIEmbeddingProvider(options, sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel, OpenAIIntentModel>();

        return services;
    }
}
