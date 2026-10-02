using System.Text.Json.Serialization;

namespace Intentum.AI.SystemOne;

/// <summary>Answer for a single typed question returned by a System One engine.</summary>
public sealed class SystemOneAnswer
{
    /// <summary>Most likely option for a choice question.</summary>
    [JsonPropertyName("choice")]
    public string? Choice { get; init; }

    /// <summary>Probability per option for a choice/score question.</summary>
    [JsonPropertyName("probabilities")]
    public IReadOnlyDictionary<string, double>? Probabilities { get; init; }

    /// <summary>Engine-reported confidence of the answer.</summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; init; }

    /// <summary>Probability of the reported answer (Laya and newer engines).</summary>
    [JsonPropertyName("answer_confidence")]
    public double? AnswerConfidence { get; init; }

    /// <summary>Probability of yes for a noul question.</summary>
    [JsonPropertyName("noul")]
    public double? Noul { get; init; }

    /// <summary>Mean level index for a score question (0-based).</summary>
    [JsonPropertyName("score")]
    public double? Score { get; init; }
}

/// <summary>Response payload of POST /v1/systemone.</summary>
public sealed class SystemOneResponse
{
    /// <summary>Answers keyed by question id.</summary>
    [JsonPropertyName("answers")]
    public IReadOnlyDictionary<string, SystemOneAnswer> Answers { get; init; } =
        new Dictionary<string, SystemOneAnswer>();
}
