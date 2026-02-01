using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Intentum.Runtime.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<IIntentEmbeddingProvider, MockEmbeddingProvider>();
        services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
        services.AddSingleton<IIntentModel>(sp =>
        {
            var e = sp.GetRequiredService<IIntentEmbeddingProvider>();
            var s = sp.GetRequiredService<IIntentSimilarityEngine>();
            return new LlmIntentModel(e, s);
        });
        services.AddSingleton(_ => new IntentPolicyBuilder()
            .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
            .Observe("Default", _ => true)
            .Build());
    })
    .Build();

host.Run();
