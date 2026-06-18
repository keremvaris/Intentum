namespace Intentum.Distributed.EventSourcing;

public interface IAggregateRoot
{
    string Id { get; }
    IReadOnlyList<IDomainEvent> Events { get; }
    void ClearEvents();
}
