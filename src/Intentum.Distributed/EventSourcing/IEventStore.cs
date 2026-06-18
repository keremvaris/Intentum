namespace Intentum.Distributed.EventSourcing;

public interface IEventStore
{
    Task AppendAsync(string streamId, IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IDomainEvent>> ReadAsync(string streamId, int fromVersion = 0, CancellationToken cancellationToken = default);
}
