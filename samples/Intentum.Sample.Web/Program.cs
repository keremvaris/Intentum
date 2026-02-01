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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Scalar.AspNetCore;
using Timer = System.Timers.Timer;

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

// Playground: named models for compare (Default = cached mock, Mock = raw mock, Strict = same engine but downgrades confidence so policy differs)
builder.Services.AddSingleton<IPlaygroundModelRegistry>(sp =>
{
    var similarity = sp.GetRequiredService<IIntentSimilarityEngine>();
    var defaultModel = sp.GetRequiredService<IIntentModel>();
    var mockOnly = new LlmIntentModel(new MockEmbeddingProvider(), similarity);
    var strictModel = new StrictConfidenceIntentModel(defaultModel);
    return new PlaygroundModelRegistry(new Dictionary<string, IIntentModel>
    {
        ["Default"] = defaultModel,
        ["Mock"] = mockOnly,
        ["Strict"] = strictModel
    });
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
builder.Services.AddIntentTreeExplainer();

// Dashboard: in-memory config + SSE broadcaster
builder.Services.AddSingleton<DashboardConfigStore>();
builder.Services.AddSingleton<SseInferenceBroadcaster>();

// Fraud simulation: state + hosted service
builder.Services.AddSingleton<FraudSimulationState>();
builder.Services.AddHostedService<FraudSimulationService>();

builder.Services.AddHttpClient();

// Add Intentum ASP.NET Core integration
builder.Services.AddIntentum();
builder.Services.AddIntentumHealthChecks();

builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = o.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();
});
builder.Services.AddControllersWithViews();
builder.Services.AddServerSideBlazor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Dashboard: serve HTML before any other middleware (avoids 403 from auth/filters)
var dashboardHtmlEarly = """
<!DOCTYPE html>
<html lang="tr">
<head><meta charset="utf-8"/><meta name="viewport" content="width=device-width, initial-scale=1"/><base href="/dashboard/"/><title>Intentum Dashboard</title><link rel="stylesheet" href="/dashboard.css"/><script src="https://cdn.jsdelivr.net/npm/echarts@5/dist/echarts.min.js"></script><script src="/echarts-interop.js"></script></head>
<body><div id="app">Yükleniyor…</div><script src="/dashboard/_framework/blazor.server.js" autostart="false"></script><script>Blazor.start({ configureSignalR: function (b) { b.withUrl("/dashboard/_blazor"); } });</script></body>
</html>
""";
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Method != "GET" || !ctx.Request.Path.StartsWithSegments("/dashboard")) { await next(ctx); return; }
    var path = ctx.Request.Path.Value ?? "";
    if (path == "/dashboard") { ctx.Response.Redirect("/dashboard/", false); return; }
    if (path == "/dashboard/" || path.StartsWith("/dashboard/", StringComparison.Ordinal))
    {
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.StatusCode = 200;
        await ctx.Response.WriteAsync(dashboardHtmlEarly);
        return;
    }
    await next(ctx);
});

// Ensure in-memory database is created (schema)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IntentumDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseExceptionHandler(err => err.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
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

app.UseAuthorization();
app.UseDefaultFiles();
// Blazor at /dashboard: /dashboard/_framework/* → /_framework/* so static files serve it
app.Use(next => async ctx =>
{
    if (ctx.Request.Path.StartsWithSegments("/dashboard/_framework"))
        ctx.Request.Path = "/_framework" + ctx.Request.Path.Value!["/dashboard/_framework".Length..];
    await next(ctx);
});
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub("/dashboard/_blazor");
app.MapGet("/dashboard", () => Results.Redirect("/dashboard/")).ExcludeFromDescription();
// Dashboard: minimal API returns HTML directly (no MVC/Razor pipeline → no 403)
var dashboardHtml = """
    <!DOCTYPE html>
    <html lang="tr">
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <base href="/dashboard/" />
        <title>Intentum Dashboard</title>
        <link rel="stylesheet" href="/dashboard.css" />
        <script src="https://cdn.jsdelivr.net/npm/echarts@5/dist/echarts.min.js"></script>
        <script src="/echarts-interop.js"></script>
    </head>
    <body>
        <div id="app">Yükleniyor…</div>
        <script src="/dashboard/_framework/blazor.server.js" autostart="false"></script>
        <script>Blazor.start({ configureSignalR: function (b) { b.withUrl("/dashboard/_blazor"); } });</script>
    </body>
    </html>
    """;
app.MapGet("/dashboard/", () => Results.Content(dashboardHtml, "text/html; charset=utf-8")).AllowAnonymous().ExcludeFromDescription();
app.MapGet("/dashboard/{*path}", (string? path) => { _ = path; return Results.Content(dashboardHtml, "text/html; charset=utf-8"); }).AllowAnonymous().ExcludeFromDescription();
app.MapControllers();

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

// --- Intent explainability (feature contribution + rule trace + confidence breakdown) ---
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
        ConfidenceScore = intent.Confidence.Score,
        intent.Reasoning,
        Explanation = explanation,
        SignalContributions = contributions.Select(c => new { c.Source, c.Description, c.Weight, c.ContributionPercent })
    });
}).WithName("ExplainIntent").Produces(200);

// --- Intent decision tree (root-cause: decision, matched rule, signals, behavior summary) ---
app.MapPost("/api/intent/explain-tree", (InferIntentRequest req, IIntentModel model, IntentPolicy policy, IIntentTreeExplainer treeExplainer) =>
{
    var space = new BehaviorSpace();
    foreach (var e in req.Events)
        space.Observe(new BehaviorEvent(e.Actor, e.Action, DateTimeOffset.UtcNow));
    var intent = model.Infer(space);
    var tree = treeExplainer.GetIntentTree(intent, policy, space);
    return Results.Json(tree);
}).WithName("ExplainIntentTree").Produces(200);

// --- Playground: compare multiple providers (events → intent + decision per provider) ---
app.MapPost("/api/intent/playground/compare", (PlaygroundCompareRequest req, IPlaygroundModelRegistry registry, IntentPolicy policy) =>
{
    var space = new BehaviorSpace();
    foreach (var e in req.Events)
        space.Observe(new BehaviorEvent(e.Actor, e.Action, DateTimeOffset.UtcNow));
    var providers = req.Providers?.Count > 0 ? req.Providers : registry.GetModelNames();
    var results = new List<PlaygroundCompareResult>();
    foreach (var name in providers)
    {
        if (!registry.TryGetModel(name, out var model) || model is null) continue;
        var intent = model.Infer(space);
        var decision = intent.Decide(policy);
        results.Add(new PlaygroundCompareResult(
            name,
            intent.Name,
            intent.Confidence.Level,
            intent.Confidence.Score,
            decision.ToString()));
    }
    return Results.Json(new PlaygroundCompareResponse(results));
}).WithName("PlaygroundCompare").Produces(200);

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

app.MapGet("/api/intent/analytics/timeline/{entityId}", async (
    string entityId,
    DateTimeOffset? from,
    DateTimeOffset? to,
    IIntentAnalytics analytics) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
    var end = to ?? DateTimeOffset.UtcNow;
    var timeline = await analytics.GetIntentTimelineAsync(entityId, start, end);
    return Results.Json(timeline);
}).WithName("GetIntentTimeline").Produces(200);

app.MapGet("/api/intent/analytics/graph/{entityId}", async (
    string entityId,
    DateTimeOffset? from,
    DateTimeOffset? to,
    IIntentAnalytics analytics) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
    var end = to ?? DateTimeOffset.UtcNow;
    var snapshot = await analytics.GetIntentGraphSnapshotAsync(entityId, start, end);
    return Results.Json(snapshot);
}).WithName("GetIntentGraphSnapshot").Produces(200);

// --- Dashboard: overview (active intents, dominant, global confidence, signal rate) ---
const int DashboardOverviewWindowMinutes = 15;
app.MapGet("/api/dashboard/overview", async (
    string? _,
    IIntentHistoryRepository historyRepository) =>
{
    var end = DateTimeOffset.UtcNow;
    var start = end.AddMinutes(-DashboardOverviewWindowMinutes);
    var records = await historyRepository.GetByTimeWindowAsync(start, end);
    var list = records.ToList();

    if (list.Count == 0)
    {
        return Results.Json(new
        {
            activeIntents = Array.Empty<object>(),
            dominantIntent = (string?)null,
            globalConfidenceScore = 0.0,
            signalRate = 0
        });
    }

    var byIntent = list
        .GroupBy(r => r.IntentName, StringComparer.OrdinalIgnoreCase)
        .Select(g => new { Name = g.Key, Confidence = g.Average(x => x.ConfidenceScore), Count = g.Count() })
        .OrderByDescending(x => x.Confidence)
        .Take(10)
        .ToList();

    var activeIntents = byIntent.Select(x => new { name = x.Name, confidence = Math.Round(x.Confidence, 4) }).ToList<object>();
    var dominantIntent = byIntent.OrderByDescending(x => x.Count).First().Name;
    var globalConfidenceScore = Math.Round(list.Average(r => r.ConfidenceScore), 4);
    var lastMinuteStart = end.AddMinutes(-1);
    var signalRate = list.Count(r => r.RecordedAt >= lastMinuteStart);

    return Results.Json(new
    {
        activeIntents,
        dominantIntent,
        globalConfidenceScore,
        signalRate
    });
}).WithName("GetDashboardOverview").Produces(200);

// --- Dashboard: signals (history filtered by intent + time) ---
app.MapGet("/api/signals", async (
    string? intent,
    DateTimeOffset? from,
    DateTimeOffset? to,
    int? limit,
    IIntentHistoryRepository historyRepository) =>
{
    var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
    var end = to ?? DateTimeOffset.UtcNow;
    var take = Math.Clamp(limit ?? 50, 1, 200);
    var records = await historyRepository.GetByTimeWindowAsync(start, end);
    var filtered = string.IsNullOrWhiteSpace(intent)
        ? records
        : records.Where(r => string.Equals(r.IntentName, intent.Trim(), StringComparison.OrdinalIgnoreCase));
    var ordered = filtered.OrderByDescending(r => r.RecordedAt).Take(take).ToList();
    var items = ordered.Select(r =>
    {
        var meta = r.Metadata;
        var eventsSummary = meta != null && (meta.TryGetValue("EventsSummary", out var es) || meta.TryGetValue("eventsSummary", out es) || meta.TryGetValue("Source", out es) || meta.TryGetValue("source", out es))
            ? es.ToString() ?? "—"
            : "—";
        return new
        {
            r.Id,
            r.IntentName,
            r.RecordedAt,
            eventsSummary,
            r.ConfidenceLevel,
            decision = r.Decision.ToString(),
            r.ConfidenceScore
        };
    });
    return Results.Json(items);
}).WithName("GetSignals").Produces(200);

// --- Dashboard: config (GET/PUT) ---
app.MapGet("/api/dashboard/config", (DashboardConfigStore store) =>
{
    var c = store.Get();
    return Results.Json(new
    {
        c.ConfidenceThreshold,
        c.SlidingWindowMinutes,
        c.DecayFactor,
        c.Provider
    });
}).WithName("GetDashboardConfig").Produces(200);

app.MapPut("/api/dashboard/config", (DashboardConfigRequest? req, DashboardConfigStore store) =>
{
    var current = store.Get();
    var threshold = req?.ConfidenceThreshold ?? current.ConfidenceThreshold;
    var window = req?.SlidingWindowMinutes ?? current.SlidingWindowMinutes;
    var decay = req?.DecayFactor ?? current.DecayFactor;
    var provider = req?.Provider ?? current.Provider;
    store.Set(new DashboardConfig(threshold, window, decay, provider));
    return Results.Json(store.Get());
}).WithName("PutDashboardConfig").Produces(200);

// --- Dashboard: SSE stream (fraud sim / overview push) ---
app.MapGet("/api/dashboard/stream", async (
    HttpContext ctx,
    SseInferenceBroadcaster broadcaster,
    CancellationToken cancellationToken) =>
{
    ctx.Response.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache";
    ctx.Response.Headers.Connection = "keep-alive";
    await ctx.Response.StartAsync(cancellationToken);
    var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ctx.RequestAborted);
    await foreach (var bytes in broadcaster.SubscribeAsync(cts.Token))
    {
        await ctx.Response.Body.WriteAsync(bytes, cts.Token);
        await ctx.Response.Body.FlushAsync(cts.Token);
    }
}).WithName("GetDashboardStream").Produces(200);

// --- Fraud simulation: start / stop / status ---
app.MapPost("/api/fraud-simulation/start", (FraudSimulationState state) =>
{
    state.Start();
    return Results.Ok(new { running = true });
}).WithName("FraudSimulationStart").Produces(200);

app.MapPost("/api/fraud-simulation/stop", (FraudSimulationState state) =>
{
    state.Stop();
    return Results.Ok(new { running = false });
}).WithName("FraudSimulationStop").Produces(200);

app.MapGet("/api/fraud-simulation/status", (FraudSimulationState state) =>
{
    return Results.Json(new
    {
        running = state.Running,
        eventsPerMinute = state.EventsPerMinute,
        lastInferenceAt = state.LastInferenceAt
    });
}).WithName("FraudSimulationStatus").Produces(200);

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
var mockTimer = new Timer(30_000) { AutoReset = true };
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
