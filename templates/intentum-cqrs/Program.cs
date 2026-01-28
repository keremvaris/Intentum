using System.Reflection;
using FluentValidation;
using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Intentum.Runtime.Policy;
using MediatR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Intentum.Cqrs.Web.Behaviors.ValidationBehavior<,>));
builder.Services.AddSingleton<IIntentEmbeddingProvider, MockEmbeddingProvider>();
builder.Services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
builder.Services.AddSingleton<IIntentModel>(sp =>
{
    var e = sp.GetRequiredService<IIntentEmbeddingProvider>();
    var s = sp.GetRequiredService<IIntentSimilarityEngine>();
    return new LlmIntentModel(e, s);
});
builder.Services.AddSingleton(_ => new IntentPolicy()
    .AddRule(new PolicyRule("HighConfidenceAllow", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("MediumConfidenceObserve", i => i.Confidence.Level == "Medium", PolicyDecision.Observe))
    .AddRule(new PolicyRule("LowConfidenceWarn", i => i.Confidence.Level == "Low", PolicyDecision.Warn)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => "Intentum CQRS + Intentum sample. See /scalar for API docs.");
app.Run();
public partial class Program;
