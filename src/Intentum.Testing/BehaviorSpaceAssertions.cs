using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;
using Xunit;

namespace Intentum.Testing;

/// <summary>
/// Assertion helpers for BehaviorSpace testing.
/// </summary>
public static class BehaviorSpaceAssertions
{
    /// <summary>
    /// Asserts that the behavior space contains a specific event.
    /// </summary>
    public static void ContainsEvent(
        BehaviorSpace space,
        string actor,
        string action)
    {
        Assert.Contains(space.Events, e => { _ = e; return e.Actor == actor && e.Action == action; });
    }

    /// <summary>
    /// Asserts that the behavior space contains a specific number of events.
    /// </summary>
    public static void HasEventCount(BehaviorSpace space, int expectedCount)
    {
        Assert.Equal(expectedCount, space.Events.Count);
    }

    /// <summary>
    /// Asserts that the behavior space contains events for a specific actor.
    /// </summary>
    public static void ContainsActor(BehaviorSpace space, string actor)
    {
        Assert.Contains(space.Events, e => { _ = e; return e.Actor == actor; });
    }

    /// <summary>
    /// Asserts that the behavior space contains a specific action.
    /// </summary>
    public static void ContainsAction(BehaviorSpace space, string action)
    {
        Assert.Contains(space.Events, e => { _ = e; return e.Action == action; });
    }
}

/// <summary>
/// Assertion helpers for Intent testing.
/// </summary>
public static class IntentAssertions
{
    /// <summary>
    /// Asserts that the intent has a specific confidence level.
    /// </summary>
    public static void HasConfidenceLevel(Intent intent, string expectedLevel)
    {
        Assert.Equal(expectedLevel, intent.Confidence.Level);
    }

    /// <summary>
    /// Asserts that the intent has a confidence score within a range.
    /// </summary>
    public static void HasConfidenceScore(Intent intent, double minScore, double maxScore)
    {
        Assert.InRange(intent.Confidence.Score, minScore, maxScore);
    }

    /// <summary>
    /// Asserts that the intent has at least one signal.
    /// </summary>
    public static void HasSignals(Intent intent)
    {
        Assert.NotEmpty(intent.Signals);
    }

    /// <summary>
    /// Asserts that the intent has a specific number of signals.
    /// </summary>
    public static void HasSignalCount(Intent intent, int expectedCount)
    {
        Assert.Equal(expectedCount, intent.Signals.Count);
    }

    /// <summary>
    /// Asserts that the intent contains a signal with a specific description.
    /// </summary>
    public static void ContainsSignal(Intent intent, string description)
    {
        Assert.Contains(intent.Signals, s => s.Description.Contains(description, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Assertion helpers for PolicyDecision testing.
/// </summary>
public static class PolicyDecisionAssertions
{
    /// <summary>
    /// Asserts that the decision is one of the allowed values.
    /// </summary>
    public static void IsOneOf(PolicyDecision decision, params PolicyDecision[] allowedDecisions)
    {
        Assert.Contains(decision, allowedDecisions);
    }

    /// <summary>
    /// Asserts that the decision is Allow.
    /// </summary>
    public static void IsAllow(PolicyDecision decision)
    {
        Assert.Equal(PolicyDecision.Allow, decision);
    }

    /// <summary>
    /// Asserts that the decision is Block.
    /// </summary>
    public static void IsBlock(PolicyDecision decision)
    {
        Assert.Equal(PolicyDecision.Block, decision);
    }

    /// <summary>
    /// Asserts that the decision is not Block.
    /// </summary>
    public static void IsNotBlock(PolicyDecision decision)
    {
        Assert.NotEqual(PolicyDecision.Block, decision);
    }
}
