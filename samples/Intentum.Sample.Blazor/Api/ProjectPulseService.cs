using Intentum.Core.Behavior;
using Intentum.Persistence.Repositories;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Background service: runs Project Pulse (The Pulse) demo variant, broadcasts steps via SSE.
/// </summary>
public sealed class ProjectPulseService(
    IServiceProvider services,
    ProjectPulseState state,
    ProjectPulseBroadcaster broadcaster,
    ILogger<ProjectPulseService> logger) : BackgroundService
{
    private const string EntityId = "ProjectPulse";

    private static readonly IntentPolicy ProjectPulsePolicy = new IntentPolicyBuilder()
        .Block("BurnoutBlock", i => i.Name.Contains("Burnout", StringComparison.OrdinalIgnoreCase) && i.Confidence.Score > 0.85)
        .Warn("BurnoutWarn", i => i.Name.Contains("Burnout", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("TechnicalDebt", StringComparison.OrdinalIgnoreCase))
        .RequireAuth("ScopeCreep", i => i.Name.Contains("ScopeCreep", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("FeatureScope", StringComparison.OrdinalIgnoreCase))
        .Escalate("DependencyBlocked", i => i.Name.Contains("Dependency", StringComparison.OrdinalIgnoreCase) || i.Name.Contains("CriticalDependency", StringComparison.OrdinalIgnoreCase))
        .Allow("TeamOnTrack", i => i.Name.Contains("TeamOnTrack", StringComparison.OrdinalIgnoreCase))
        .Observe("Default", _ => true)
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
                if (!scope.ServiceProvider.GetRequiredService<IPlaygroundModelRegistry>().TryGetModel("ProjectPulse", out var model) || model is null)
                {
                    logger.LogWarning("ProjectPulse intent model not found");
                    state.Stop();
                    continue;
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                if (!state.Running) break;

                var variant = state.CurrentVariant;
                var space = new BehaviorSpace();
                space.SetMetadata("ProjectId", "FlightReservation");
                space.SetMetadata("Variant", variant);
                var baseTime = DateTimeOffset.UtcNow;
                var eventsSummary = new List<string>();

                var events = GetEventsForVariant(variant, baseTime);
                for (var i = 0; i < events.Count; i++)
                {
                    if (!state.Running) break;
                    var (evt, summary) = events[i];
                    space.Observe(evt);
                    eventsSummary.Add(summary);
                    state.SetStep(i + 1);

                    var intent = model.Infer(space);
                    var decision = intent.Decide(ProjectPulsePolicy);
                    var history = scope.ServiceProvider.GetRequiredService<IIntentHistoryRepository>();
                    var behaviorSpaceId = Guid.NewGuid().ToString();
                    var metadata = new Dictionary<string, object>
                    {
                        ["Source"] = "ProjectPulse",
                        ["Variant"] = variant,
                        ["EventsSummary"] = string.Join("; ", eventsSummary)
                    };
                    var id = await history.SaveAsync(behaviorSpaceId, intent, decision, metadata, EntityId);

                    broadcaster.Broadcast(new
                    {
                        Source = "ProjectPulse",
                        Id = id,
                        Variant = variant,
                        EventIndex = i + 1,
                        TotalSteps = events.Count,
                        IntentName = intent.Name,
                        ConfidenceLevel = intent.Confidence.Level,
                        ConfidenceScore = intent.Confidence.Score,
                        Decision = decision.ToString(),
                        EventsSummary = string.Join("; ", eventsSummary),
                        RecordedAt = DateTimeOffset.UtcNow,
                        intent.Reasoning,
                        Final = i == events.Count - 1
                    });

                    if (i < events.Count - 1)
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }

                state.Stop();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Project Pulse step failed");
                state.Stop();
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private static List<(BehaviorEvent Evt, string Summary)> GetEventsForVariant(string variant, DateTimeOffset baseTime)
    {
        return variant switch
        {
            ProjectPulseVariants.VariantA => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Dev_A", "TaskCompleted", baseTime, new Dictionary<string, object> { ["DaysLate"] = 2, ["SprintId"] = "Sprint_7", ["TaskStatus"] = "DoneLate" }), "TaskCompleted (DaysLate=2)"),
                (new BehaviorEvent("Dev_B", "PR_Created", baseTime.AddMinutes(1), new Dictionary<string, object> { ["HourOfDay"] = 23, ["Branch"] = "hotfix/login", ["IsHotfix"] = true }), "PR_Created (23:45)"),
                (new BehaviorEvent("PM_X", "Message_Sent", baseTime.AddMinutes(2), new Dictionary<string, object> { ["SentimentScore"] = -0.7, ["Channel"] = "general" }), "Message_Sent (SentimentScore=-0.7)"),
                (new BehaviorEvent("Dev_A", "Estimate_Increased", baseTime.AddMinutes(3), new Dictionary<string, object> { ["TaskId"] = "T-101", ["OldPoints"] = 3, ["NewPoints"] = 6 }), "Estimate_Increased (3→6 pts)")
            },
            ProjectPulseVariants.VariantB => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Dev_A", "TaskCompleted", baseTime, new Dictionary<string, object> { ["DaysLate"] = 0, ["SprintId"] = "Sprint_7" }), "TaskCompleted (on time)"),
                (new BehaviorEvent("Dev_B", "PR_Created", baseTime.AddMinutes(1), new Dictionary<string, object> { ["HourOfDay"] = 14, ["Branch"] = "feature/checkout" }), "PR_Created (14:00)"),
                (new BehaviorEvent("PM_X", "Message_Sent", baseTime.AddMinutes(2), new Dictionary<string, object> { ["SentimentScore"] = 0.3, ["Channel"] = "general" }), "Message_Sent (SentimentScore=0.3)")
            },
            ProjectPulseVariants.VariantC => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Dev_A", "Estimate_Increased", baseTime, new Dictionary<string, object> { ["TaskId"] = "T-102", ["OldPoints"] = 2, ["NewPoints"] = 5 }), "Estimate_Increased (2→5)"),
                (new BehaviorEvent("PM_X", "Meeting_Attended", baseTime.AddMinutes(1), new Dictionary<string, object> { ["DurationMinutes"] = 60, ["IsRecurring"] = true }), "Meeting_Attended (60 min)"),
                (new BehaviorEvent("Dev_B", "Estimate_Increased", baseTime.AddMinutes(2), new Dictionary<string, object> { ["TaskId"] = "T-103", ["OldPoints"] = 1, ["NewPoints"] = 4 }), "Estimate_Increased (1→4)"),
                (new BehaviorEvent("PM_X", "Meeting_Attended", baseTime.AddMinutes(3), new Dictionary<string, object> { ["DurationMinutes"] = 45 }), "Meeting_Attended (45 min)")
            },
            ProjectPulseVariants.VariantD => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Dev_A", "TaskBlocked", baseTime, new Dictionary<string, object> { ["BlockedByTaskId"] = "EXT-1", ["BlockerRole"] = "External", ["BlockedSinceHours"] = 24 }), "TaskBlocked (External)"),
                (new BehaviorEvent("DevOps", "Deployment_Failed", baseTime.AddMinutes(1), new Dictionary<string, object> { ["Environment"] = "Staging", ["ErrorCategory"] = "Timeout", ["RollbackDone"] = true }), "Deployment_Failed (Staging)"),
                (new BehaviorEvent("Dev_A", "TaskBlocked", baseTime.AddMinutes(2), new Dictionary<string, object> { ["BlockedByTaskId"] = "EXT-2", ["BlockerRole"] = "External" }), "TaskBlocked (External)")
            },
            _ => GetEventsForVariant(ProjectPulseVariants.VariantA, baseTime)
        };
    }
}
