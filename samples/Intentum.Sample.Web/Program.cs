using System.Reflection;
using FluentValidation;
using Intentum.AI.Caching;
using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Analytics;
using Intentum.Analytics.Models;
using Intentum.AspNetCore;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Explainability;
using Intentum.Persistence.EntityFramework;
using Intentum.Persistence.Repositories;
using Intentum.Runtime;
using Intentum.Runtime.Policy;
using Intentum.Runtime.RateLimiting;
using Intentum.Sample.Web.Api;
using Intentum.Sample.Web.Behaviors;
using Intentum.Sample.Web.Features.CarbonFootprintCalculation.Commands;
using Intentum.Sample.Web.Features.CarbonFootprintCalculation.Queries;
using Intentum.Sample.Web.Features.CarbonFootprintCalculation.Validators;
using Intentum.Sample.Web.Features.GreenwashingDetection;
using Intentum.Sample.Web.Features.OrderPlacement.Commands;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CalculateCarbonCommandValidator>();

// Intentum: mock embedding + similarity + model + policy
// Add caching for better performance
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IEmbeddingCache>(sp =>
    new MemoryEmbeddingCache(sp.GetRequiredService<IMemoryCache>()));

// Use CompositeSimilarityEngine combining SimpleAverage and Cosine
builder.Services.AddSingleton<IIntentSimilarityEngine>(_ =>
{
    var engines = new (IIntentSimilarityEngine Engine, double Weight)[]
    {
        (new SimpleAverageSimilarityEngine(), 1.0),
        (new CosineSimilarityEngine(), 1.5)
    };
    return new CompositeSimilarityEngine(engines);
});

builder.Services.AddSingleton<IIntentEmbeddingProvider>(sp =>
{
    var provider = new MockEmbeddingProvider();
    var cache = sp.GetRequiredService<IEmbeddingCache>();
    return new CachedEmbeddingProvider(provider, cache);
});

builder.Services.AddSingleton<IIntentModel>(sp =>
{
    var embedding = sp.GetRequiredService<IIntentEmbeddingProvider>();
    var similarity = sp.GetRequiredService<IIntentSimilarityEngine>();
    return new LlmIntentModel(embedding, similarity);
});

// Use fluent API for policy building with new decision types
builder.Services.AddSingleton(_ =>
{
    return new IntentPolicyBuilder()
        .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3)
        .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
        .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive", StringComparison.OrdinalIgnoreCase)))
        .RateLimit("HighFrequency", i => i.Signals.Count > 10)
        .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
        .Observe("MediumConfidence", i => i.Confidence.Level == "Medium")
        .Warn("LowConfidenceWarn", i => i.Confidence.Level == "Low")
        .Build();
});

// Rate limiting (in-memory; use distributed implementation for multi-node)
builder.Services.AddSingleton<IRateLimiter, MemoryRateLimiter>();

// Persistence (in-memory for sample; use real DB in production)
builder.Services.AddIntentumPersistenceInMemory("IntentumSampleWeb");

// Reporting & analytics
builder.Services.AddIntentAnalytics();

// Explainability (signal contributions, human-readable explanation)
builder.Services.AddScoped<IIntentExplainer, IntentExplainer>();

// Add Intentum ASP.NET Core integration
builder.Services.AddIntentum();
builder.Services.AddIntentumHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure in-memory database is created (schema)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IntentumDbContext>();
    await db.Database.EnsureCreatedAsync();
}

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

// Intentum behavior observation middleware
app.UseIntentumBehaviorObservation(new BehaviorObservationOptions
{
    Enabled = true,
    IncludeHeaders = false,
    GetActor = _ => "http",
    GetAction = ctx => $"{ctx.Request.Method.ToLowerInvariant()}_{ctx.Request.Path.Value?.Replace("/", "_")}"
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapOpenApi();
app.MapScalarApiReference();

// Health checks endpoint
app.MapHealthChecks("/health");

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

// --- Intentum: infer + rate limit + persist (for analytics) ---
app.MapPost("/api/intent/infer", async (
    InferIntentRequest req,
    IIntentModel model,
    IntentPolicy policy,
    IRateLimiter rateLimiter,
    IIntentHistoryRepository historyRepository) =>
{
    var space = new BehaviorSpace();
    foreach (var e in req.Events)
        space.Observe(new BehaviorEvent(e.Actor, e.Action, DateTimeOffset.UtcNow));

    var intent = model.Infer(space);
    var rateLimitOptions = new RateLimitOptions("intent-infer", 100, TimeSpan.FromMinutes(1));
    var (decision, rateLimitResult) = await intent.DecideWithRateLimitAsync(
        policy, rateLimiter, rateLimitOptions);

    var behaviorSpaceId = Guid.NewGuid().ToString();
    var metadata = new Dictionary<string, object>
    {
        ["Source"] = "Niyet çıkarımı (form)",
        ["EventsSummary"] = string.Join(", ", req.Events.Select(e => $"{e.Actor}:{e.Action}"))
    };
    var id = await historyRepository.SaveAsync(behaviorSpaceId, intent, decision, metadata);

    return Results.Ok(new InferIntentResponse(
        Decision: decision.ToString(),
        Confidence: intent.Confidence.Level,
        RateLimitAllowed: rateLimitResult?.Allowed ?? true,
        RateLimitCurrent: rateLimitResult?.CurrentCount,
        RateLimitLimit: rateLimitResult?.Limit,
        HistoryId: id));
}).WithName("InferIntent").Produces(200);

// --- Intent explainability (infer + explanation + signal contributions) ---
app.MapPost("/api/intent/explain", (InferIntentRequest req, IIntentModel model, IIntentExplainer explainer) =>
{
    var space = new BehaviorSpace();
    foreach (var e in req.Events)
        space.Observe(new BehaviorEvent(e.Actor, e.Action, DateTimeOffset.UtcNow));
    var intent = model.Infer(space);
    var explanation = explainer.GetExplanation(intent, maxSignals: 5);
    var contributions = explainer.GetSignalContributions(intent);
    return Results.Ok(new
    {
        IntentName = intent.Name,
        Confidence = intent.Confidence.Level,
        Explanation = explanation,
        SignalContributions = contributions.Select(c => new { c.Source, c.Description, c.Weight, c.ContributionPercent })
    });
}).WithName("ExplainIntent").Produces(200);

// --- Reporting & analytics ---
app.MapGet("/api/intent/analytics/summary", async (
    DateTimeOffset? from,
    DateTimeOffset? to,
    IIntentAnalytics analytics) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
    var end = to ?? DateTimeOffset.UtcNow;
    var summary = await analytics.GetSummaryAsync(start, end, TimeSpan.FromHours(1));
    if (summary.Anomalies.Count == 0 && summary.TotalInferences > 0)
    {
        var now = DateTimeOffset.UtcNow;
        var mockAnomalies = new List<AnomalyReport>
        {
            new AnomalyReport(
                "Demo",
                "Hareket var; gerçek anomali eşiği aşılmadı (demo). Block/Volume spike veya Low confidence kümesi yok.",
                now,
                start,
                end,
                0.3,
                new Dictionary<string, object> { ["TotalInferences"] = summary.TotalInferences })
        };
        summary = summary with { Anomalies = mockAnomalies };
    }
    return Results.Json(summary);
}).WithName("GetAnalyticsSummary").Produces(200);

app.MapGet("/api/intent/analytics/export/json", async (
    DateTimeOffset? from,
    DateTimeOffset? to,
    IIntentAnalytics analytics) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
    var end = to ?? DateTimeOffset.UtcNow;
    var json = await analytics.ExportToJsonAsync(start, end);
    return Results.Text(json, "application/json");
}).WithName("ExportAnalyticsJson").Produces(200);

app.MapGet("/api/intent/analytics/export/csv", async (
    DateTimeOffset? from,
    DateTimeOffset? to,
    IIntentAnalytics analytics) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
    var end = to ?? DateTimeOffset.UtcNow;
    var csv = await analytics.ExportToCsvAsync(start, end);
    return Results.Text(csv, "text/csv");
}).WithName("ExportAnalyticsCsv").Produces(200);

// --- Greenwashing: report text (+ dil, görsel) → behavior space → intent → policy → suggested actions ---
app.MapPost("/api/greenwashing/analyze", (GreenwashingAnalyzeRequest req) =>
{
    var report = req.Report ?? "";
    var sourceType = req.SourceType ?? "Report";
    var language = req.Language;
    var imageBase64 = req.ImageBase64;

    var space = SustainabilityReporter.AnalyzeReport(report, language);
    GreenwashingVisualResult? visualResult = null;
    if (!string.IsNullOrWhiteSpace(imageBase64))
    {
        var (greenScore, _) = GreenwashingImageAnalyzer.AnalyzeAndAugment(imageBase64, space);
        visualResult = new GreenwashingVisualResult(greenScore, greenScore >= 0.38 ? "Yeşil baskın (demo)" : "Düşük yeşillik");
    }

    var model = new GreenwashingIntentModel();
    var intent = model.Infer(space);
    var policy = new IntentPolicyBuilder()
        .Escalate("CriticalGreenwashing", i => i is { Name: "ActiveGreenwashing", Confidence.Score: >= 0.7 })
        .Warn("NeedsVerification", i => i.Name == "StrategicObfuscation" || i is { Name: "SelectiveDisclosure", Confidence.Score: >= 0.5 })
        .Observe("Monitor", i => i.Confidence.Score > 0.3)
        .Allow("LowRisk", _ => true)
        .Build();
    var decision = intent.Decide(policy);
    var actions = SustainabilitySolutionGenerator.Suggest(intent, space, decision);

    var now = DateTimeOffset.UtcNow;
    var blockchainRef = "0x" + Guid.NewGuid().ToString("N")[..32];
    var scope3Summary = GreenwashingScope3Mock.Get();
    GreenwashingSourceMetadata? metadata;
    if (sourceType is "SocialMedia" or "PressRelease" or "InvestorPresentation")
    {
        metadata = sourceType switch
        {
            "SocialMedia" => new GreenwashingSourceMetadata(
                "Sosyal medya (mock)",
                language ?? "TR",
                Scope3Verified: false,
                Scope3Summary: null,
                blockchainRef,
                now),
            "PressRelease" => new GreenwashingSourceMetadata(
                "Basın bülteni (mock)",
                language ?? "TR",
                Scope3Verified: false,
                Scope3Summary: scope3Summary,
                blockchainRef,
                now),
            "InvestorPresentation" => new GreenwashingSourceMetadata(
                "Yatırımcı sunumu (mock)",
                language ?? "EN",
                Scope3Verified: true,
                Scope3Summary: scope3Summary,
                blockchainRef,
                now),
            _ => null
        };
    }
    else
    {
        metadata = new GreenwashingSourceMetadata(
            "Rapor",
            language ?? "TR",
            false,
            null,
            blockchainRef,
            now);
    }

    var recentItem = new GreenwashingRecentItem(
        blockchainRef,
        report.Length > 80 ? report[..80] + "…" : report,
        intent.Name,
        decision.ToString(),
        sourceType,
        language,
        now);
    GreenwashingRecentStore.Add(recentItem);

    return Results.Ok(new GreenwashingAnalyzeResponse(
        intent.Name,
        intent.Confidence.Level,
        intent.Confidence.Score,
        decision.ToString(),
        intent.Signals.Select(s => s.Description).ToList(),
        actions,
        blockchainRef,
        metadata,
        visualResult));
}).WithName("AnalyzeGreenwashing").Produces(200);

// --- Greenwashing: son analizler (gerçek zamanlı mock) ---
app.MapGet("/api/greenwashing/recent", (int? limit) =>
{
    var take = Math.Clamp(limit ?? 15, 1, 50);
    return Results.Json(GreenwashingRecentStore.GetRecent(take));
}).WithName("GetGreenwashingRecent").Produces(200);

// Periyodik mock analiz (gerçek zamanlı akış hissi)
var mockTimer = new System.Timers.Timer(30_000) { AutoReset = true };
mockTimer.Elapsed += (_, _) => GreenwashingRecentStore.AddMockEntry();
app.Lifetime.ApplicationStarted.Register(() => mockTimer.Start());
app.Lifetime.ApplicationStopping.Register(() => mockTimer.Stop());

// --- Intent history (for monitoring "recent inferences") ---
app.MapGet("/api/intent/history", async (
    DateTimeOffset? from,
    DateTimeOffset? to,
    int? limit,
    IIntentHistoryRepository historyRepository) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
    var end = to ?? DateTimeOffset.UtcNow;
    var records = await historyRepository.GetByTimeWindowAsync(start, end);
    var take = Math.Clamp(limit ?? 50, 1, 100);
    var ordered = records.OrderByDescending(r => r.RecordedAt).Take(take).ToList();
    return Results.Json(ordered);
}).WithName("GetIntentHistory").Produces(200);

await app.RunAsync();
