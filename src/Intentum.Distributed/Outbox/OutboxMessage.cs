namespace Intentum.Distributed.Outbox;

public sealed record OutboxMessage(
    Guid Id,
    string Type,
    string Payload,
    DateTime CreatedAt,
    bool Processed = false);
