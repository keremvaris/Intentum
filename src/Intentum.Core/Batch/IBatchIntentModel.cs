using Intentum.Core.Behavior;
using Intentum.Core.Intents;

namespace Intentum.Core.Batch;

/// <summary>
/// Interface for batch intent inference operations.
/// </summary>
public interface IBatchIntentModel
{
    /// <summary>
    /// Infers intents for multiple behavior spaces in batch.
    /// </summary>
    IReadOnlyList<Intent> InferBatch(IReadOnlyCollection<BehaviorSpace> behaviorSpaces);

    /// <summary>
    /// Infers intents for multiple behavior spaces in parallel.
    /// </summary>
    Task<IReadOnlyList<Intent>> InferBatchAsync(
        IReadOnlyCollection<BehaviorSpace> behaviorSpaces,
        CancellationToken cancellationToken = default);
}
