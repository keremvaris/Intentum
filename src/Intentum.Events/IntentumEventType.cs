namespace Intentum.Events;

/// <summary>
/// Types of intent-related events that can be dispatched (telemetry model).
/// </summary>
public enum IntentumEventType
{
    /// <summary>Intent was inferred from a behavior space.</summary>
    IntentInferred,

    /// <summary>Policy decision was applied (e.g. Allow, Block).</summary>
    PolicyDecisionChanged,

    /// <summary>Intent was created (first time inferred for this context).</summary>
    IntentCreated,

    /// <summary>Intent was updated (name, signals, or reasoning changed).</summary>
    IntentUpdated,

    /// <summary>Intent was resolved (final intent after pipeline or multi-stage).</summary>
    IntentResolved,

    /// <summary>Confidence score or level changed (e.g. after re-evaluation).</summary>
    ConfidenceChanged
}
