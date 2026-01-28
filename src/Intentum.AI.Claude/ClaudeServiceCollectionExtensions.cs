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
        options.Validate();
        services.AddSingleton(options);
        var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", options.ApiVersion);
        services.AddSingleton(httpClient);
        services.AddSingleton<IIntentEmbeddingProvider, ClaudeEmbeddingProvider>();
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        if (options.UseMessagesScoring)
        {
            services.AddSingleton<IIntentModel, ClaudeMessageIntentModel>();
        }
        else
        {
            services.AddSingleton<IIntentModel, ClaudeIntentModel>();
        }

        return services;
    }
}
