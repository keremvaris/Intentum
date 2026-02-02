using Intentum.Core.Behavior;

namespace Intentum.Sample.Blazor.Api;

// ========== Senaryo 1: Project Pulse ==========
/// <summary>Sprint hedefi ilerlemesi.</summary>
public enum SprintStatus { OnTrack, AtRisk, Overdue, Cancelled }

/// <summary>Görev durumu.</summary>
public enum TaskStatus { Todo, InProgress, Blocked, Done, DoneLate }

/// <summary>Kullanıcı rolü.</summary>
public enum UserRole { Dev, QA, PM, Designer, DevOps }

/// <summary>Takım pulse durumu (çıkarılan intent ile eşleşir).</summary>
public enum TeamPulseStatus { Healthy, Stressed, BurnoutRisk, ScopeCreep, DependencyBlocked }

// ========== Senaryo 2: Customer Journey ==========
/// <summary>Funnel aşaması.</summary>
public enum JourneyStage { Awareness, Consideration, Comparison, Decision, PostPurchase }

/// <summary>Referral kaynağı.</summary>
public enum ReferralSource { Google_Ad, LinkedIn_Organic, Blog_Newsletter, Direct }

// ========== Senaryo 3: Moderation ==========
/// <summary>Kullanıcı moderasyon geçmişi.</summary>
public enum UserModerationStatus { New, Warned, Observed, Restricted, Banned }

/// <summary>Thread rolü.</summary>
public enum ThreadRole { OP, Participant, Moderator }

// ========== Senaryo 4: Adaptive Tutor ==========
/// <summary>Öğrenme ilerleme durumu.</summary>
public enum LearningProgressStatus { OnTrack, Struggling, AtRisk, NeedsIntervention }

/// <summary>Zorlanma tipi (quiz/aktivite).</summary>
public enum StruggleType { Syntax_Error, Logic_Flaw, Timeout, PartialAnswer }

// ========== Senaryo 5: Digital Twin ==========
/// <summary>Bileşen sağlık durumu.</summary>
public enum ComponentHealthStatus { Healthy, Degraded, Failing, Offline }

/// <summary>Dış girdi durumu.</summary>
public enum ExternalInputStatus { None, WeatherAlert, DemandSpike, SupplierDelay, MaintenanceWindow }

// ========== Ortak: BehaviorEvent üretimi ==========
/// <summary>Domain olaylarını BehaviorEvent'e dönüştürür; Actor/Action + Metadata kullanır.</summary>
public static class IntentLayerEventBuilder
{
    public static BehaviorEvent ToBehaviorEvent(string actor, string action, DateTimeOffset occurredAt, IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new BehaviorEvent(actor, action, occurredAt, metadata is { Count: > 0 } ? new Dictionary<string, object>(metadata) : null);
    }
}
