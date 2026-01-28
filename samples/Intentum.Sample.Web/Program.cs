using System.Reflection;
using FluentValidation;
using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Intentum.Runtime.Policy;
using Intentum.Sample.Web.Behaviors;
using Intentum.Sample.Web.Features.CarbonFootprintCalculation.Commands;
using Intentum.Sample.Web.Features.CarbonFootprintCalculation.Queries;
using Intentum.Sample.Web.Features.CarbonFootprintCalculation.Validators;
using Intentum.Sample.Web.Features.OrderPlacement.Commands;
using MediatR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CalculateCarbonCommandValidator>();

// Intentum: mock embedding + similarity + model + policy
builder.Services.AddSingleton<IIntentEmbeddingProvider, MockEmbeddingProvider>();
builder.Services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
builder.Services.AddSingleton<IIntentModel>(sp =>
{
    var embedding = sp.GetRequiredService<IIntentEmbeddingProvider>();
    var similarity = sp.GetRequiredService<IIntentSimilarityEngine>();
    return new LlmIntentModel(embedding, similarity);
});
builder.Services.AddSingleton(_ =>
{
    return new IntentPolicy()
        .AddRule(new PolicyRule(
            "ExcessiveRetryBlock",
            i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
            PolicyDecision.Block))
        .AddRule(new PolicyRule(
            "HighConfidenceAllow",
            i => i.Confidence.Level is "High" or "Certain",
            PolicyDecision.Allow))
        .AddRule(new PolicyRule(
            "MediumConfidenceObserve",
            i => i.Confidence.Level == "Medium",
            PolicyDecision.Observe))
        .AddRule(new PolicyRule(
            "LowConfidenceWarn",
            i => i.Confidence.Level == "Low",
            PolicyDecision.Warn));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(err => err.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    if (ex is ValidationException validationException)
    {
        ctx.Response.StatusCode = 400;
        ctx.Response.ContentType = "application/json";
        var errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        await ctx.Response.WriteAsJsonAsync(new { errors });
    }
}));
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapOpenApi();
app.MapScalarApiReference();

// Carbon footprint (with Intentum)
app.MapPost("/api/carbon/calculate", async (CalculateCarbonCommand cmd, IMediator mediator) =>
{
    var result = await mediator.Send(cmd);
    return result switch
    {
        CalculateCarbonOk ok => Results.Ok(ok),
        CalculateCarbonError err => Results.BadRequest(new { error = err.Error }),
        _ => Results.BadRequest()
    };
}).WithName("CalculateCarbon").Produces(200).Produces(400);

// Carbon report (query, no Intentum)
app.MapGet("/api/carbon/report/{reportId}", async (string reportId, IMediator mediator) =>
{
    var result = await mediator.Send(new GetCarbonReportQuery(reportId));
    return result is not null ? Results.Ok(result) : Results.NotFound();
}).WithName("GetCarbonReport").Produces(200).Produces(404);

// Order placement (second feature, no Intentum in handler)
app.MapPost("/api/orders", async (PlaceOrderCommand cmd, IMediator mediator) =>
{
    var result = await mediator.Send(cmd);
    return Results.Created($"/api/orders/{result.OrderId}", result);
}).WithName("PlaceOrder").Produces(201).Produces(400);

await app.RunAsync();
