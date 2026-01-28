namespace Intentum.Events;

/// <summary>
/// Types of intent-related events that can be dispatched.
/// </summary>
public enum IntentumEventType
{
    /// <summary>Intent was inferred from a behavior space.</summary>
    IntentInferred,

    /// <summary>Policy decision was applied (e.g. Allow, Block).</summary>
    PolicyDecisionChanged
}
