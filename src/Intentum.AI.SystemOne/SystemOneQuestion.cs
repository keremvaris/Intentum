using System.Text.Json.Serialization;

namespace Intentum.AI.SystemOne;

/// <summary>A typed System One question: choice, noul (yes/no) or score.</summary>
/// <param name="Type">Question type: choice, noul or score.</param>
/// <param name="Instructions">Human/model instructions describing the question.</param>
/// <param name="Criteria">Choice: option → description map; score: ordered level list; noul: optional.</param>
public sealed record SystemOneQuestion(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("instructions")] string Instructions,
    [property: JsonPropertyName("criteria")] object? Criteria = null)
{
    /// <summary>A choice question over named options.</summary>
    public static SystemOneQuestion Choice(string instructions, IReadOnlyDictionary<string, string?> criteria) =>
        new("choice", instructions, criteria);

    /// <summary>A score question over ordered levels (lowest first).</summary>
    public static SystemOneQuestion Score(string instructions, IReadOnlyList<string> criteria) =>
        new("score", instructions, criteria);

    /// <summary>A yes/no question; the answer carries the probability of yes.</summary>
    public static SystemOneQuestion Noul(string instructions) =>
        new("noul", instructions);
}
