namespace Intentum.Distributed.EventSourcing;

public interface IDomainEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
}
