using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Core.Batch;

/// <summary>
/// Batch intent model that processes multiple behavior spaces efficiently.
/// </summary>
public sealed class BatchIntentModel : IBatchIntentModel
{
    private readonly IIntentModel _innerModel;

    public BatchIntentModel(IIntentModel innerModel)
    {
        _innerModel = innerModel ?? throw new ArgumentNullException(nameof(innerModel));
    }

    public IReadOnlyList<Intent> InferBatch(IReadOnlyCollection<BehaviorSpace>? behaviorSpaces)
    {
        if (behaviorSpaces == null || behaviorSpaces.Count == 0)
            return [];

        return behaviorSpaces
            .Select(space => _innerModel.Infer(space))
            .ToList();
    }

    public async Task<IReadOnlyList<Intent>> InferBatchAsync(
        IReadOnlyCollection<BehaviorSpace>? behaviorSpaces,
        CancellationToken cancellationToken = default)
    {
        if (behaviorSpaces == null || behaviorSpaces.Count == 0)
            return [];

        var tasks = behaviorSpaces.Select(async space =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield(); // Allow async context switching
            cancellationToken.ThrowIfCancellationRequested();
            return _innerModel.Infer(space);
        });

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}
