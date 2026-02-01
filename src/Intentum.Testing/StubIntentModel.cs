using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Testing;

/// <summary>
/// Stub implementation of <see cref="IIntentModel"/> for unit tests.
/// Returns a configurable intent (name, confidence, reasoning) for any input.
/// Use when testing policy, chaining, or pipeline without a real model.
/// </summary>
public sealed class StubIntentModel : IIntentModel
{
    private readonly Func<BehaviorSpace, Intent> _infer;

    /// <summary>
    /// Creates a stub that always returns the same intent.
    /// </summary>
    public StubIntentModel(string intentName, double confidenceScore = 0.8, string? reasoning = null)
    {
        var confidence = IntentConfidence.FromScore(confidenceScore);
        _infer = _ => new Intent(intentName, [], confidence, reasoning);
    }

    /// <summary>
    /// Creates a stub that uses a custom function to produce intent from behavior space.
    /// </summary>
    public StubIntentModel(Func<BehaviorSpace, Intent> infer)
    {
        _infer = infer ?? throw new ArgumentNullException(nameof(infer));
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
        => _infer(behaviorSpace);
}
