using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Experiments;

/// <summary>
/// A/B experiment: run behavior spaces through multiple variants with deterministic
/// hash-based traffic splitting and statistical significance reporting.
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
    /// Runs the experiment with deterministic hash-based variant assignment.
    /// Each behavior space is assigned a variant based on a stable hash of its identity,
    /// ensuring the same space always gets the same variant across runs.
    /// </summary>
    public Task<IReadOnlyList<ExperimentResult>> RunAsync(
        IReadOnlyList<BehaviorSpace> behaviorSpaces,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_variants.Count == 0)
            throw new InvalidOperationException("Add at least one variant with AddVariant.");

        var split = NormalizeSplit(_trafficSplit, _variants.Count);
        var results = new List<ExperimentResult>();

        foreach (var space in behaviorSpaces)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var identity = GetSpaceIdentity(space);
            var variantIndex = SelectVariantByHash(identity, split);
            var variant = _variants[variantIndex];
            var intent = variant.Model.Infer(space);
            var decision = intent.Decide(variant.Policy);
            results.Add(new ExperimentResult(variant.Name, identity, intent, decision));
        }

        return Task.FromResult<IReadOnlyList<ExperimentResult>>(results);
    }

    /// <summary>
    /// Synchronous batch run.
    /// </summary>
    public IReadOnlyList<ExperimentResult> Run(IReadOnlyList<BehaviorSpace> behaviorSpaces)
        => RunAsync(behaviorSpaces).GetAwaiter().GetResult();

    /// <summary>
    /// Computes basic statistical significance between two variants using a chi-square test
    /// on the distribution of decisions.
    /// </summary>
    public static ExperimentSignificance ComputeSignificance(
        IReadOnlyList<ExperimentResult> results,
        string variantA,
        string variantB)
    {
        var groupA = results.Where(r => r.VariantName == variantA).ToList();
        var groupB = results.Where(r => r.VariantName == variantB).ToList();

        if (groupA.Count == 0 || groupB.Count == 0)
            return new ExperimentSignificance(0, 1.0, false);

        var allDecisions = groupA.Select(r => r.Decision)
            .Concat(groupB.Select(r => r.Decision))
            .Distinct()
            .ToList();

        var chiSquare = 0.0;
        var totalA = (double)groupA.Count;
        var totalB = (double)groupB.Count;
        var total = totalA + totalB;

        foreach (var decision in allDecisions)
        {
            var observedA = groupA.Count(r => r.Decision == decision);
            var observedB = groupB.Count(r => r.Decision == decision);
            var totalForDecision = observedA + observedB;

            var expectedA = totalForDecision * (totalA / total);
            var expectedB = totalForDecision * (totalB / total);

            if (expectedA > 0) chiSquare += Math.Pow(observedA - expectedA, 2) / expectedA;
            if (expectedB > 0) chiSquare += Math.Pow(observedB - expectedB, 2) / expectedB;
        }

        var degreesOfFreedom = Math.Max(1, allDecisions.Count - 1);
        var pValue = ChiSquarePValue(chiSquare, degreesOfFreedom);

        return new ExperimentSignificance(chiSquare, pValue, pValue < 0.05);
    }

    private static string GetSpaceIdentity(BehaviorSpace space)
    {
        var entityId = space.GetMetadata<string>("entityId");
        if (!string.IsNullOrEmpty(entityId))
            return entityId;

        var sessionId = space.GetMetadata<string>("sessionId");
        if (!string.IsNullOrEmpty(sessionId))
            return sessionId;

        return string.Join("|", space.Events.Select(e => $"{e.Actor}:{e.Action}"));
    }

    private static int SelectVariantByHash(string identity, int[] split)
    {
        var hash = StableHash(identity);
        var bucket = (int)(((uint)hash) % 100);
        var cumulative = 0;
        for (var i = 0; i < split.Length; i++)
        {
            cumulative += split[i];
            if (bucket < cumulative)
                return i;
        }
        return split.Length - 1;
    }

    private static int StableHash(string input)
    {
        unchecked
        {
            var hash = 5381;
            foreach (var c in input)
                hash = ((hash << 5) + hash) + c;
            return hash;
        }
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

    /// <summary>
    /// Approximation of chi-square p-value using the Wilson-Hilferty transformation.
    /// </summary>
    private static double ChiSquarePValue(double chiSquare, int degreesOfFreedom)
    {
        if (chiSquare <= 0 || degreesOfFreedom <= 0) return 1.0;

        var k = degreesOfFreedom;
        var z = Math.Pow(chiSquare / k, 1.0 / 3) - (1.0 - 2.0 / (9.0 * k));
        z /= Math.Sqrt(2.0 / (9.0 * k));

        var pValue = 0.5 * Erfc(z / Math.Sqrt(2));
        return Math.Clamp(pValue, 0, 1);
    }

    private static double Erfc(double x)
    {
        var t = 1.0 / (1.0 + 0.5 * Math.Abs(x));
        var tau = t * Math.Exp(
            -x * x - 1.26551223 +
            t * (1.00002368 +
            t * (0.37409196 +
            t * (0.09678418 +
            t * (-0.18628806 +
            t * (0.27886807 +
            t * (-1.13520398 +
            t * (1.48851587 +
            t * (-0.82215223 +
            t * 0.17087277)))))))));
        return x >= 0 ? tau : 2 - tau;
    }
}

/// <summary>
/// Result of statistical significance test between two experiment variants.
/// </summary>
public sealed record ExperimentSignificance(
    double ChiSquare,
    double PValue,
    bool IsSignificant
);
