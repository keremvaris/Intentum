using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Events;

/// <summary>
/// Payload for intent-related events (inferred intent and policy decision).
/// </summary>
/// <param name="BehaviorSpaceId">Optional behavior space identifier.</param>
/// <param name="Intent">The inferred intent.</param>
/// <param name="Decision">The policy decision.</param>
/// <param name="RecordedAt">When the event occurred.</param>
public sealed record IntentEventPayload(
    string? BehaviorSpaceId,
    Intent Intent,
    PolicyDecision Decision,
    DateTimeOffset RecordedAt);
