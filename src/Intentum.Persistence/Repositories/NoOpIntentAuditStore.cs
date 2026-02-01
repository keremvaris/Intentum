using Intentum.Runtime.Audit;

namespace Intentum.Persistence.Repositories;

/// <summary>
/// No-op implementation of <see cref="IIntentAuditStore"/> that discards audit events.
/// Use when audit trail is not required or a custom store is not yet implemented.
/// </summary>
public sealed class NoOpIntentAuditStore : IIntentAuditStore
{
    /// <inheritdoc />
    public Task AppendAsync(IntentAuditEvent auditEvent, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
