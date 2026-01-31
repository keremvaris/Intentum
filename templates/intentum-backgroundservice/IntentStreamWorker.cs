using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Streaming;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.BackgroundService;

public sealed class IntentStreamWorker : BackgroundService
{
    private readonly IBehaviorStreamConsumer _consumer;
    private readonly IIntentModel _model;
    private readonly IntentPolicy _policy;

    public IntentStreamWorker(
        IBehaviorStreamConsumer consumer,
        IIntentModel model,
        IntentPolicy policy)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // In production, replace IBehaviorStreamConsumer with Kafka/Event Hubs consumer.
        // This template uses MemoryBehaviorStreamConsumer: post batches via PostAsync from another service or timer.
        await foreach (var batch in _consumer.ReadAllAsync(stoppingToken))
        {
            var space = new BehaviorSpace();
            foreach (var e in batch.Events)
                space.Observe(e);
            var intent = _model.Infer(space);
            var decision = intent.Decide(_policy);
            // TODO: persist or publish (intent, decision); e.g. IIntentHistoryRepository, message bus
        }
    }
}
