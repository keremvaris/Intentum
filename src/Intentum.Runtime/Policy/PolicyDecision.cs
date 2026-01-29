namespace Intentum.Runtime.Policy;

/// <summary>
/// Policy decision types for intent evaluation.
/// </summary>
public enum PolicyDecision
{
    /// <summary>Allow the action to proceed.</summary>
    Allow,

    /// <summary>Observe the action but allow it to proceed.</summary>
    Observe,

    /// <summary>Warn about the action but allow it to proceed.</summary>
    Warn,

    /// <summary>Block the action.</summary>
    Block,

    /// <summary>Escalate to a higher level for review.</summary>
    Escalate,

    /// <summary>Require additional authentication before proceeding.</summary>
    RequireAuth,

    /// <summary>Apply rate limiting to the action.</summary>
    RateLimit
}
