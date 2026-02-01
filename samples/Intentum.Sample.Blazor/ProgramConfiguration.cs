using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using Intentum.Sample.Blazor.Api;
using Intentum.Sample.Blazor.Behaviors;
using Intentum.Sample.Blazor.Components;
using Intentum.Sample.Blazor.Features.CarbonFootprintCalculation.Commands;
using Intentum.Sample.Blazor.Features.CarbonFootprintCalculation.Queries;
using Intentum.Sample.Blazor.Features.CarbonFootprintCalculation.Validators;
using Intentum.Sample.Blazor.Features.GreenwashingDetection;
using Intentum.Sample.Blazor.Features.OrderPlacement.Commands;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Scalar.AspNetCore;

namespace Intentum.Sample.Blazor;

internal static class ProgramConfiguration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddValidatorsFromAssemblyContaining<CalculateCarbonCommandValidator>();

        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IEmbeddingCache>(sp =>
            new MemoryEmbeddingCache(sp.GetRequiredService<IMemoryCache>()));

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

        builder.Services.AddSingleton(_ =>
            new IntentPolicyBuilder()
                .Block("ExcessiveRetry", i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3)
                .Escalate("LowConfidence", i => i.Confidence.Level == "Low")
                .RequireAuth("SensitiveAction", i => i.Signals.Any(s => s.Description.Contains("sensitive", StringComparison.OrdinalIgnoreCase)))
                .RateLimit("HighFrequency", i => i.Signals.Count > 10)
                .Allow("HighConfidence", i => i.Confidence.Level is "High" or "Certain")
                .Observe("MediumConfidence", i => i.Confidence.Level == "Medium")
                .Warn("LowConfidenceWarn", i => i.Confidence.Level == "Low")
                .Build());

        builder.Services.AddSingleton<IRateLimiter, MemoryRateLimiter>();
        builder.Services.AddIntentumPersistenceInMemory("IntentumSampleBlazor");
        builder.Services.AddIntentAnalytics();
        builder.Services.AddScoped<IIntentExplainer, IntentExplainer>();
        builder.Services.AddIntentTreeExplainer();

        builder.Services.AddSingleton<DashboardConfigStore>();
        builder.Services.AddSingleton<SseInferenceBroadcaster>();
        builder.Services.AddSingleton<FraudSimulationState>();
        builder.Services.AddHostedService<FraudSimulationService>();
        builder.Services.AddSingleton<SustainabilitySimulationState>();
        builder.Services.AddSingleton<SustainabilityTimelineBroadcaster>();
        builder.Services.AddHostedService<SustainabilityTimelineService>();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("Intentum.SSE", c => c.Timeout = Timeout.InfiniteTimeSpan);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<HttpClient>(sp =>
        {
            var accessor = sp.GetRequiredService<IHttpContextAccessor>();
            var req = accessor.HttpContext?.Request;
            var scheme = req != null ? req.Scheme : "http";
            var host = req != null ? req.Host.Value : "localhost:5018";
            return new HttpClient { BaseAddress = new Uri($"{scheme}://{host}") };
        });
        builder.Services.AddIntentum();
        builder.Services.AddIntentumHealthChecks();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
    }

    public static void ConfigureMiddleware(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/Error", createScopeForErrors: true);

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

        app.UseIntentumBehaviorObservation(new BehaviorObservationOptions
        {
            Enabled = true,
            IncludeHeaders = false,
            GetActor = _ => "http",
            GetAction = ctx => $"{ctx.Request.Method.ToLowerInvariant()}_{ctx.Request.Path.Value?.Replace("/", "_")}"
        });

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseAntiforgery();
    }

    public static void MapEndpoints(WebApplication app)
    {
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapHealthChecks("/health");

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

        app.MapGet("/api/carbon/report/{reportId}", async (string reportId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCarbonReportQuery(reportId));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        }).WithName("GetCarbonReport").Produces(200).Produces(404);

        app.MapPost("/api/orders", async (PlaceOrderCommand cmd, IMediator mediator) =>
        {
            var result = await mediator.Send(cmd);
            return Results.Created($"/api/orders/{result.OrderId}", result);
        }).WithName("PlaceOrder").Produces(201).Produces(400);

        MapIntentEndpoints(app);
        MapAnalyticsEndpoints(app);
        MapDashboardEndpoints(app);
        MapSimulationEndpoints(app);
        MapGreenwashingEndpoints(app);
        MapHistoryEndpoints(app);
    }

    private static void MapIntentEndpoints(WebApplication app)
    {
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
            var (decision, rateLimitResult) = await intent.DecideWithRateLimitAsync(policy, rateLimiter, rateLimitOptions);

            var behaviorSpaceId = Guid.NewGuid().ToString();
            var metadata = new Dictionary<string, object>
            {
                ["Source"] = "Niyet çıkarımı (form)",
                ["EventsSummary"] = string.Join(", ", req.Events.Select(e => $"{e.Actor}:{e.Action}"))
            };
            var id = await historyRepository.SaveAsync(behaviorSpaceId, intent, decision, metadata, req.EntityId);

            return Results.Ok(new InferIntentResponse(
                Decision: decision.ToString(),
                Confidence: intent.Confidence.Level,
                RateLimitAllowed: rateLimitResult?.Allowed ?? true,
                RateLimitCurrent: rateLimitResult?.CurrentCount,
                RateLimitLimit: rateLimitResult?.Limit,
                HistoryId: id));
        }).WithName("InferIntent").Produces(200);

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

        app.MapPost("/api/intent/explain-tree", (InferIntentRequest req, IIntentModel model, IntentPolicy policy, IIntentTreeExplainer treeExplainer) =>
        {
            var space = new BehaviorSpace();
            foreach (var e in req.Events)
                space.Observe(new BehaviorEvent(e.Actor, e.Action, DateTimeOffset.UtcNow));
            var intent = model.Infer(space);
            var tree = treeExplainer.GetIntentTree(intent, policy, space);
            var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } };
            return Results.Json(tree, jsonOpts);
        }).WithName("ExplainIntentTree").Produces(200);

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
                results.Add(new PlaygroundCompareResult(name, intent.Name, intent.Confidence.Level, intent.Confidence.Score, decision.ToString()));
            }
            return Results.Json(new PlaygroundCompareResponse(results));
        }).WithName("PlaygroundCompare").Produces(200);
    }

    private static void MapAnalyticsEndpoints(WebApplication app)
    {
        app.MapGet("/api/intent/analytics/summary", async (DateTimeOffset? from, DateTimeOffset? to, IIntentAnalytics analytics) =>
        {
            var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
            var end = to ?? DateTimeOffset.UtcNow;
            var summary = await analytics.GetSummaryAsync(start, end, TimeSpan.FromHours(1));
            if (summary.Anomalies.Count == 0 && summary.TotalInferences > 0)
            {
                var now = DateTimeOffset.UtcNow;
                summary = summary with { Anomalies = new List<AnomalyReport> { new AnomalyReport("Demo", "Hareket var; gerçek anomali eşiği aşılmadı (demo).", now, start, end, 0.3, new Dictionary<string, object> { ["TotalInferences"] = summary.TotalInferences }) } };
            }
            return Results.Json(summary);
        }).WithName("GetAnalyticsSummary").Produces(200);

        app.MapGet("/api/intent/analytics/export/json", async (DateTimeOffset? from, DateTimeOffset? to, IIntentAnalytics analytics) =>
        {
            var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
            var end = to ?? DateTimeOffset.UtcNow;
            var json = await analytics.ExportToJsonAsync(start, end);
            return Results.Text(json, "application/json");
        }).WithName("ExportAnalyticsJson").Produces(200);

        app.MapGet("/api/intent/analytics/export/csv", async (DateTimeOffset? from, DateTimeOffset? to, IIntentAnalytics analytics) =>
        {
            var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
            var end = to ?? DateTimeOffset.UtcNow;
            var csv = await analytics.ExportToCsvAsync(start, end);
            return Results.Text(csv, "text/csv");
        }).WithName("ExportAnalyticsCsv").Produces(200);

        app.MapGet("/api/intent/analytics/timeline/{entityId}", async (string entityId, DateTimeOffset? from, DateTimeOffset? to, IIntentAnalytics analytics) =>
        {
            var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
            var end = to ?? DateTimeOffset.UtcNow;
            var timeline = await analytics.GetIntentTimelineAsync(entityId, start, end);
            return Results.Json(timeline);
        }).WithName("GetIntentTimeline").Produces(200);

        app.MapGet("/api/intent/analytics/graph/{entityId}", async (string entityId, DateTimeOffset? from, DateTimeOffset? to, IIntentAnalytics analytics) =>
        {
            var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
            var end = to ?? DateTimeOffset.UtcNow;
            var snapshot = await analytics.GetIntentGraphSnapshotAsync(entityId, start, end);
            return Results.Json(snapshot);
        }).WithName("GetIntentGraphSnapshot").Produces(200);
    }

    private static void MapDashboardEndpoints(WebApplication app)
    {
        const int DashboardOverviewWindowMinutes = 15;
        app.MapGet("/api/dashboard/overview", async (string? _, IIntentHistoryRepository historyRepository) =>
        {
            var end = DateTimeOffset.UtcNow;
            var start = end.AddMinutes(-DashboardOverviewWindowMinutes);
            var records = await historyRepository.GetByTimeWindowAsync(start, end);
            var list = records.ToList();
            if (list.Count == 0)
                return Results.Json(new { activeIntents = Array.Empty<object>(), dominantIntent = (string?)null, globalConfidenceScore = 0.0, signalRate = 0 });

            var byIntent = list.GroupBy(r => r.IntentName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Name = g.Key, Confidence = g.Average(x => x.ConfidenceScore), Count = g.Count() })
                .OrderByDescending(x => x.Confidence).Take(10).ToList();
            var activeIntents = byIntent.Select(x => new { name = x.Name, confidence = Math.Round(x.Confidence, 4) }).ToList<object>();
            var dominantIntent = byIntent.OrderByDescending(x => x.Count).First().Name;
            var globalConfidenceScore = Math.Round(list.Average(r => r.ConfidenceScore), 4);
            var signalRate = list.Count(r => r.RecordedAt >= end.AddMinutes(-1));
            return Results.Json(new { activeIntents, dominantIntent, globalConfidenceScore, signalRate });
        }).WithName("GetDashboardOverview").Produces(200);

        app.MapGet("/api/signals", async (string? intent, DateTimeOffset? from, DateTimeOffset? to, int? limit, IIntentHistoryRepository historyRepository) =>
        {
            var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
            var end = to ?? DateTimeOffset.UtcNow;
            var take = Math.Clamp(limit ?? 50, 1, 200);
            var records = await historyRepository.GetByTimeWindowAsync(start, end);
            var filtered = string.IsNullOrWhiteSpace(intent) ? records : records.Where(r => string.Equals(r.IntentName, (intent).Trim(), StringComparison.OrdinalIgnoreCase));
            var ordered = filtered.OrderByDescending(r => r.RecordedAt).Take(take).ToList();
            var items = ordered.Select(r =>
            {
                var meta = r.Metadata;
                var eventsSummary = meta != null && (meta.TryGetValue("EventsSummary", out var es) || meta.TryGetValue("eventsSummary", out es) || meta.TryGetValue("Source", out es) || meta.TryGetValue("source", out es)) ? es.ToString() ?? "—" : "—";
                return new { r.Id, r.IntentName, r.RecordedAt, eventsSummary, r.ConfidenceLevel, decision = r.Decision.ToString(), r.ConfidenceScore };
            });
            return Results.Json(items);
        }).WithName("GetSignals").Produces(200);

        app.MapGet("/api/dashboard/config", (DashboardConfigStore store) =>
        {
            var c = store.Get();
            return Results.Json(new { c.ConfidenceThreshold, c.SlidingWindowMinutes, c.DecayFactor, c.Provider });
        }).WithName("GetDashboardConfig").Produces(200);

        app.MapPut("/api/dashboard/config", (DashboardConfigRequest? req, DashboardConfigStore store) =>
        {
            var current = store.Get();
            store.Set(new DashboardConfig(req?.ConfidenceThreshold ?? current.ConfidenceThreshold, req?.SlidingWindowMinutes ?? current.SlidingWindowMinutes, req?.DecayFactor ?? current.DecayFactor, req?.Provider ?? current.Provider));
            return Results.Json(store.Get());
        }).WithName("PutDashboardConfig").Produces(200);

        app.MapGet("/api/dashboard/stream", async (HttpContext ctx, SseInferenceBroadcaster broadcaster, CancellationToken cancellationToken) =>
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
    }

    private static void MapSimulationEndpoints(WebApplication app)
    {
        app.MapPost("/api/fraud-simulation/start", (FraudSimulationState state) => { state.Start(); return Results.Ok(new { running = true }); }).WithName("FraudSimulationStart").Produces(200);
        app.MapPost("/api/fraud-simulation/stop", (FraudSimulationState state) => { state.Stop(); return Results.Ok(new { running = false }); }).WithName("FraudSimulationStop").Produces(200);
        app.MapGet("/api/fraud-simulation/status", (FraudSimulationState state) => Results.Json(new { state.Running, state.EventsPerMinute, state.LastInferenceAt })).WithName("FraudSimulationStatus").Produces(200);

        app.MapPost("/api/sustainability-simulation/start", (SustainabilityStartRequest? req, SustainabilitySimulationState state) =>
        {
            state.Start(req?.CompanyId ?? "shell", req?.Granularity ?? "Monthly");
            return Results.Ok(new { running = true });
        }).WithName("SustainabilitySimulationStart").Produces(200);
        app.MapPost("/api/sustainability-simulation/stop", (SustainabilitySimulationState state) =>
        {
            state.Stop();
            return Results.Ok(new { running = false });
        }).WithName("SustainabilitySimulationStop").Produces(200);
        app.MapGet("/api/sustainability-simulation/status", (SustainabilitySimulationState state) =>
            Results.Json(new { state.Running, state.CompanyId, state.Granularity, state.SimulatedAt, state.StartAt })).WithName("SustainabilitySimulationStatus").Produces(200);
        app.MapGet("/api/sustainability/stream", async (HttpContext ctx, SustainabilityTimelineBroadcaster broadcaster, CancellationToken cancellationToken) =>
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
        }).WithName("GetSustainabilityStream").Produces(200);
    }

    private static void MapGreenwashingEndpoints(WebApplication app)
    {
        app.MapPost("/api/greenwashing/analyze", (GreenwashingAnalyzeRequest req) =>
        {
            var report = req.Report ?? "";
            var space = SustainabilityReporter.AnalyzeReport(report, req.Language);
            var model = new GreenwashingIntentModel();
            var intent = model.Infer(space);
            var policy = new IntentPolicyBuilder().Escalate("CriticalGreenwashing", i => i is { Name: "ActiveGreenwashing", Confidence.Score: >= 0.7 }).Warn("NeedsVerification", i => i.Name == "StrategicObfuscation").Observe("Monitor", i => i.Confidence.Score > 0.3).Allow("LowRisk", _ => true).Build();
            var decision = intent.Decide(policy);
            var actions = SustainabilitySolutionGenerator.Suggest(intent, space, decision);
            return Results.Ok(new GreenwashingAnalyzeResponse(intent.Name, intent.Confidence.Level, intent.Confidence.Score, decision.ToString(), intent.Signals.Select(s => s.Description).ToList(), actions, "0x" + Guid.NewGuid().ToString("N")[..32], null, null));
        }).WithName("AnalyzeGreenwashing").Produces(200);

        app.MapGet("/api/greenwashing/recent", (int? limit) => Results.Json(GreenwashingRecentStore.GetRecent(Math.Clamp(limit ?? 15, 1, 50)))).WithName("GetGreenwashingRecent").Produces(200);
    }

    private static void MapHistoryEndpoints(WebApplication app)
    {
        app.MapGet("/api/intent/history", async (DateTimeOffset? from, DateTimeOffset? to, int? limit, IIntentHistoryRepository historyRepository) =>
        {
            var start = from ?? DateTimeOffset.UtcNow.AddDays(-7);
            var end = to ?? DateTimeOffset.UtcNow;
            var records = await historyRepository.GetByTimeWindowAsync(start, end);
            var take = Math.Clamp(limit ?? 50, 1, 100);
            return Results.Json(records.OrderByDescending(r => r.RecordedAt).Take(take).ToList());
        }).WithName("GetIntentHistory").Produces(200);
    }
}
