namespace Intentum.AI.Embeddings;

/// <summary>
/// Represents an embedding for a behavior key, containing source, score, and optional vector.
/// </summary>
public sealed record IntentEmbedding(
    string Source,
    double Score,
    IReadOnlyList<double>? Vector = null
);
