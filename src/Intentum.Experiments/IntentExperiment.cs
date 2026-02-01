using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Experiments;

/// <summary>
/// A/B experiment: run behavior spaces through multiple variants with traffic splitting.
/// </summary>
public sealed class IntentExperiment
{
    private readonly List<ExperimentVariant> _variants = [];
    private readonly List<int> _trafficSplit = [];

    /// <summary>
    /// Adds a variant (model + policy) to the experiment.
    /// </summary>
    public IntentExperiment AddVariant(string name, IIntentModel model, IntentPolicy policy)
    {
        _variants.Add(new ExperimentVariant(name, model ?? throw new ArgumentNullException(nameof(model)),
            policy ?? throw new ArgumentNullException(nameof(policy))));
        return this;
    }

    /// <summary>
    /// Sets traffic split percentages (e.g. 50, 50 for two variants). Must sum to 100 and match variant count.
    /// </summary>
    public IntentExperiment SplitTraffic(params int[]? percentages)
    {
        _trafficSplit.Clear();
        _trafficSplit.AddRange(percentages ?? []);
        return this;
    }

    /// <summary>
    /// Runs the experiment: each behavior space is assigned a variant by traffic split and inferred.
    /// </summary>
    /// <param name="behaviorSpaces">Behavior spaces to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>One result per behavior space (variant name, intent, decision).</returns>
    public Task<IReadOnlyList<ExperimentResult>> RunAsync(
        IReadOnlyList<BehaviorSpace> behaviorSpaces,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_variants.Count == 0)
            throw new InvalidOperationException("Add at least one variant with AddVariant.");

        var split = NormalizeSplit(_trafficSplit, _variants.Count);
        var results = new List<ExperimentResult>();

        for (var i = 0; i < behaviorSpaces.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var space = behaviorSpaces[i];
            var variantIndex = SelectVariantIndex(i, behaviorSpaces.Count, split);
            var variant = _variants[variantIndex];
            var intent = variant.Model.Infer(space);
            var decision = intent.Decide(variant.Policy);
            results.Add(new ExperimentResult(
                variant.Name,
                null,
                intent,
                decision));
        }

        return Task.FromResult<IReadOnlyList<ExperimentResult>>(results);
    }

    /// <summary>
    /// Synchronous batch run.
    /// </summary>
    public IReadOnlyList<ExperimentResult> Run(IReadOnlyList<BehaviorSpace> behaviorSpaces)
    {
        return RunAsync(behaviorSpaces).GetAwaiter().GetResult();
    }

    private static int[] NormalizeSplit(List<int> requested, int variantCount)
    {
        if (requested.Count == variantCount && requested.Sum() == 100)
            return requested.ToArray();
        var equal = 100 / variantCount;
        var remainder = 100 % variantCount;
        var split = new int[variantCount];
        for (var i = 0; i < variantCount; i++)
            split[i] = equal + (i < remainder ? 1 : 0);
        return split;
    }

    private static int SelectVariantIndex(int spaceIndex, int totalSpaces, int[] split)
    {
        var bucket = totalSpaces > 0 ? (spaceIndex * 100) / totalSpaces : 0;
        var sum = 0;
        for (var i = 0; i < split.Length; i++)
        {
            sum += split[i];
            if (bucket < sum)
                return i;
        }
        return split.Length - 1;
    }
}
