using System.Runtime.CompilerServices;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using JetBrains.Annotations;

namespace Intentum.AI;

/// <summary>
/// Streaming and batch extensions for <see cref="IIntentModel"/>.
/// </summary>
[UsedImplicitly]
public static class IntentModelStreamingExtensions
{
    /// <summary>
    /// Infers intent for each behavior space and returns results as a sequence (lazy enumeration).
    /// </summary>
    /// <param name="model">The intent model.</param>
    /// <param name="spaces">Behavior spaces to infer.</param>
    /// <returns>Sequence of inferred intents in the same order as <paramref name="spaces"/>.</returns>
    [UsedImplicitly]
    public static IEnumerable<Intent> InferMany(
        this IIntentModel model,
        IEnumerable<BehaviorSpace> spaces)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(spaces);
        return InferManyIterator(model, spaces);
    }

    private static IEnumerable<Intent> InferManyIterator(IIntentModel model, IEnumerable<BehaviorSpace> spaces)
    {
        foreach (var space in spaces)
            yield return model.Infer(space);
    }

    /// <summary>
    /// Infers intent for each behavior space as they are enumerated, yielding results as they are ready (async stream).
    /// </summary>
    /// <param name="model">The intent model.</param>
    /// <param name="spaces">Async sequence of behavior spaces.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async sequence of inferred intents in the same order as <paramref name="spaces"/>.</returns>
    [UsedImplicitly]
    public static async IAsyncEnumerable<Intent> InferManyAsync(
        this IIntentModel model,
        IAsyncEnumerable<BehaviorSpace> spaces,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(spaces);
        await foreach (var intent in InferManyAsyncIterator(model, spaces, cancellationToken))
            yield return intent;
    }

    private static async IAsyncEnumerable<Intent> InferManyAsyncIterator(
        IIntentModel model,
        IAsyncEnumerable<BehaviorSpace> spaces,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var space in spaces.WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return model.Infer(space);
        }
    }
}
