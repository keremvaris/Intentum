namespace Intentum.Distributed.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken = default);
}
