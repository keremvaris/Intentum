using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.BackgroundService;
using Intentum.Core.Contracts;
using Intentum.Runtime.Policy;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<Intentum.Core.Streaming.MemoryBehaviorStreamConsumer>();
builder.Services.AddSingleton<Intentum.Core.Streaming.IBehaviorStreamConsumer>(sp =>
    sp.GetRequiredService<Intentum.Core.Streaming.MemoryBehaviorStreamConsumer>());
builder.Services.AddSingleton<IIntentEmbeddingProvider, MockEmbeddingProvider>();
builder.Services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
builder.Services.AddSingleton<IIntentModel>(sp =>
{
    var e = sp.GetRequiredService<IIntentEmbeddingProvider>();
    var s = sp.GetRequiredService<IIntentSimilarityEngine>();
    return new LlmIntentModel(e, s);
});
builder.Services.AddSingleton(_ => new IntentPolicyBuilder()
    .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
    .Observe("Default", _ => true)
    .Build());
builder.Services.AddHostedService<IntentStreamWorker>();

var host = builder.Build();
await host.RunAsync();
