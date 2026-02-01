using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Persistence.Repositories;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Web.Api;

/// <summary>
/// Background service: generates synthetic behavior events, runs intent inference, saves to history, and pushes to SSE.
/// </summary>
public sealed class FraudSimulationService(
    IServiceProvider services,
    FraudSimulationState state,
    ILogger<FraudSimulationService> logger)
    : BackgroundService
{
    private static readonly (string Actor, string Action)[] EventPool =
    {
        ("user", "login"),
        ("user", "login_failed"),
        ("user", "retry"),
        ("user", "retry"),
        ("user", "retry"),
        ("user", "submit"),
        ("user", "high_value_transfer"),
        ("user", "password_reset_request"),
        ("customer", "add_to_cart"),
        ("customer", "checkout_start"),
        ("user", "sensitive_data_access"),
        ("system", "ip_change")
    };

    private const string EntityId = "LiveDemo";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rnd = new Random();
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
                var model = scope.ServiceProvider.GetRequiredService<IIntentModel>();
                var policy = scope.ServiceProvider.GetRequiredService<IntentPolicy>();
                var history = scope.ServiceProvider.GetRequiredService<IIntentHistoryRepository>();
                var broadcaster = scope.ServiceProvider.GetRequiredService<SseInferenceBroadcaster>();

                var count = rnd.Next(1, 4);
                var space = new BehaviorSpace();
                var eventsSummary = new List<string>();
                for (var i = 0; i < count; i++)
                {
                    var (actor, action) = EventPool[rnd.Next(EventPool.Length)];
                    space.Observe(new BehaviorEvent(actor, action, DateTimeOffset.UtcNow));
                    eventsSummary.Add($"{actor}:{action}");
                }

                var intent = model.Infer(space);
                var decision = intent.Decide(policy);
                var behaviorSpaceId = Guid.NewGuid().ToString();
                var metadata = new Dictionary<string, object> { ["EventsSummary"] = string.Join(", ", eventsSummary), ["Source"] = "FraudSimulation" };
                var id = await history.SaveAsync(behaviorSpaceId, intent, decision, metadata, EntityId);

                var now = DateTimeOffset.UtcNow;
                state.RecordInference();

                broadcaster.Broadcast(new
                {
                    Id = id,
                    IntentName = intent.Name,
                    ConfidenceLevel = intent.Confidence.Level,
                    ConfidenceScore = intent.Confidence.Score,
                    Decision = decision.ToString(),
                    EventsSummary = string.Join(", ", eventsSummary),
                    RecordedAt = now
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Fraud simulation step failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}
