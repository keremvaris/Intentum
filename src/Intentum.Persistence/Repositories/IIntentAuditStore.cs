using Intentum.Runtime.Audit;

namespace Intentum.Persistence.Repositories;

/// <summary>
/// Append-only store for intent audit events (input hash, model/policy version, override flag).
/// Implement for compliance: persist every infer/policy decision for audit trail.
/// </summary>
public interface IIntentAuditStore
{
    /// <summary>
    /// Appends an audit event to the store (non-blocking; implementations may buffer).
    /// </summary>
    Task AppendAsync(IntentAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
