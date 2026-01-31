using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.AspNetCore;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

var builder = WebApplication.CreateBuilder(args);

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
    .Observe("MediumConfidence", i => i.Confidence.Level == "Medium")
    .Warn("LowConfidence", i => i.Confidence.Level == "Low")
    .Build());

builder.Services.AddIntentum();
builder.Services.AddIntentumHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapHealthChecks("/health");

app.MapPost("/api/intent/infer", (InferRequest req, IIntentModel model, IntentPolicy policy) =>
{
    var space = new BehaviorSpace();
    foreach (var e in req.Events)
        space.Observe(new BehaviorEvent(e.Actor, e.Action, DateTimeOffset.UtcNow));
    var intent = model.Infer(space);
    var decision = intent.Decide(policy);
    return Results.Ok(new { intent.Name, intent.Confidence.Level, intent.Confidence.Score, Decision = decision.ToString() });
});

app.MapGet("/", () => "Intentum Web API. POST /api/intent/infer with body: { \"events\": [ { \"actor\": \"user\", \"action\": \"login\" } ] }. Health: /health");

app.Run();

internal record InferRequest(IReadOnlyList<EventDto> Events);
internal record EventDto(string Actor, string Action);

public partial class Program;
