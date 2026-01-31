namespace Intentum.Analytics.Models;

/// <summary>
/// A detected behavior pattern (e.g. sequence of intent names with frequency).
/// </summary>
/// <param name="Sequence">Ordered sequence (e.g. intent names "A" then "B").</param>
/// <param name="Count">Number of times this sequence was observed.</param>
public sealed record BehaviorPattern(
    IReadOnlyList<string> Sequence,
    int Count);
