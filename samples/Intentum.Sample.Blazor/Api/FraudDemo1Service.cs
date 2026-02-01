using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Persistence.Repositories;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Runs Demo 1 (Finans Dolandırıcılığı): fixed event sequence (Login + IP, ChangeContactEmail, HighValueTransfer, RequestNewCardExpressShipping),
/// Infer after each step with ChainedIntentModel (RuleBased + LlmIntentModel TimeDecay), broadcast via SSE.
/// </summary>
public sealed class FraudDemo1Service(
    IServiceProvider services,
    FraudDemo1State state,
    SseInferenceBroadcaster broadcaster,
    ILogger<FraudDemo1Service> logger) : BackgroundService
{
    private const string EntityId = "FraudDemo1";
    private const string LoginIp = "105.12.34.56";
    private const string NormalCountry = "TR";

    private static readonly IntentPolicy Demo1Policy = new IntentPolicyBuilder()
        .Block("HighFraudRisk", i => i.Confidence.Score > 0.90)
        .Escalate("HighFraudEscalate", i => i.Confidence.Score is > 0.85 and <= 0.90)
        .Observe("Review", _ => true)
        .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!state.Running)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                continue;
            }

            try
            {
                using var scope = services.CreateScope();
                if (!scope.ServiceProvider.GetRequiredService<IPlaygroundModelRegistry>().TryGetModel("Fraud", out var model) || model is null)
                {
                    logger.LogWarning("Fraud intent model not found");
                    state.Stop();
                    continue;
                }

                // Kısa gecikme: istemci "Başlat Demo 1" ile SSE'ye abone olana zaman tanır; böylece KPI/gauge/grafik/ticker ilk olayları kaçırmaz.
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                if (!state.Running) break;

                var space = new BehaviorSpace();
                space.SetMetadata("User", "Ahmet K.");
                var baseTime = DateTimeOffset.UtcNow;
                var eventsSummary = new List<string>();

                // Event 1: Login (yeni ülke IP)
                var meta1 = new Dictionary<string, object> { ["IP"] = LoginIp, ["Country"] = "NG", ["Note"] = "Yeni bir ülke" };
                space.Observe(new BehaviorEvent("user", "Login", baseTime, meta1));
                eventsSummary.Add($"Login (IP: {LoginIp}, yeni ülke)");
                state.SetStep(1);
                await BroadcastStep(scope.ServiceProvider, model, space, 1, eventsSummary);
                await Task.Delay(TimeSpan.FromSeconds(6), stoppingToken);
                if (!state.Running) break;

                // Event 2: ChangeContactEmail
                space.Observe(new BehaviorEvent("user", "ChangeContactEmail", baseTime.AddMinutes(1)));
                eventsSummary.Add("ChangeContactEmail");
                state.SetStep(2);
                await BroadcastStep(scope.ServiceProvider, model, space, 2, eventsSummary);
                await Task.Delay(TimeSpan.FromSeconds(6), stoppingToken);
                if (!state.Running) break;

                // Event 3: HighValueTransfer
                space.Observe(new BehaviorEvent("user", "HighValueTransfer", baseTime.AddMinutes(2), new Dictionary<string, object> { ["Note"] = "Maksimum limit" }));
                eventsSummary.Add("HighValueTransfer (maksimum limit)");
                state.SetStep(3);
                await BroadcastStep(scope.ServiceProvider, model, space, 3, eventsSummary);
                await Task.Delay(TimeSpan.FromSeconds(6), stoppingToken);
                if (!state.Running) break;

                // Event 4: RequestNewCardExpressShipping
                space.Observe(new BehaviorEvent("user", "RequestNewCardExpressShipping", baseTime.AddMinutes(3)));
                eventsSummary.Add("RequestNewCardExpressShipping");
                state.SetStep(4);
                await BroadcastStep(scope.ServiceProvider, model, space, 4, eventsSummary);

                var intent = model.Infer(space);
                var decision = intent.Decide(Demo1Policy);
                var history = scope.ServiceProvider.GetRequiredService<IIntentHistoryRepository>();
                var behaviorSpaceId = Guid.NewGuid().ToString();
                var metadata = new Dictionary<string, object>
                {
                    ["Source"] = "FraudDemo1",
                    ["EventsSummary"] = string.Join("; ", eventsSummary),
                    ["LoginIp"] = LoginIp,
                    ["NormalLocation"] = NormalCountry
                };
                var id = await history.SaveAsync(behaviorSpaceId, intent, decision, metadata, EntityId, stoppingToken);

                broadcaster.Broadcast(new
                {
                    Source = "FraudDemo1",
                    Id = id,
                    EventIndex = 4,
                    IntentName = intent.Name,
                    ConfidenceLevel = intent.Confidence.Level,
                    ConfidenceScore = intent.Confidence.Score,
                    Decision = decision.ToString(),
                    EventsSummary = string.Join("; ", eventsSummary),
                    LoginIp,
                    NormalLocation = NormalCountry,
                    RecordedAt = DateTimeOffset.UtcNow,
                    intent.Reasoning,
                    Final = true
                });

                state.Stop();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Fraud Demo1 step failed");
                state.Stop();
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task BroadcastStep(IServiceProvider scoped, IIntentModel model, BehaviorSpace space, int eventIndex, List<string> eventsSummary)
    {
        var intent = model.Infer(space);
        var decision = intent.Decide(Demo1Policy);
        var history = scoped.GetRequiredService<IIntentHistoryRepository>();
        var behaviorSpaceId = Guid.NewGuid().ToString();
        var metadata = new Dictionary<string, object>
        {
            ["Source"] = "FraudDemo1",
            ["EventsSummary"] = string.Join("; ", eventsSummary),
            ["LoginIp"] = LoginIp,
            ["NormalLocation"] = NormalCountry
        };
        var id = await history.SaveAsync(behaviorSpaceId, intent, decision, metadata, EntityId);

        broadcaster.Broadcast(new
        {
            Source = "FraudDemo1",
            Id = id,
            EventIndex = eventIndex,
            IntentName = intent.Name,
            ConfidenceLevel = intent.Confidence.Level,
            ConfidenceScore = intent.Confidence.Score,
            Decision = decision.ToString(),
            EventsSummary = string.Join("; ", eventsSummary),
            LoginIp,
            NormalLocation = NormalCountry,
            RecordedAt = DateTimeOffset.UtcNow,
            intent.Reasoning,
            Final = false
        });
    }
}
