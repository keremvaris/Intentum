using Intentum.AI.Catalog;
using Intentum.AI.Embeddings;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Pipeline;

namespace Intentum.AI.Models;

/// <summary>
/// AI inference step that classifies intent by comparing behavior embeddings against
/// a catalog of known intent definitions using cosine similarity (nearest neighbor).
/// Unlike LlmInferenceStep which always returns "AI-Inferred-Intent", this step
/// returns the actual matched intent name from the catalog.
/// </summary>
public sealed class CatalogInferenceStep : IIntentInferenceStep
{
    private readonly IIntentEmbeddingProvider _embeddingProvider;
    private readonly IntentCatalog _catalog;

    public CatalogInferenceStep(
        IIntentEmbeddingProvider embeddingProvider,
        IntentCatalog catalog)
    {
        _embeddingProvider = embeddingProvider ?? throw new ArgumentNullException(nameof(embeddingProvider));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <inheritdoc />
    public IntentInferenceResult Infer(BehaviorSpace behaviorSpace, BehaviorVector vector)
    {
        var embeddings = vector.Dimensions.Keys
            .Select(_embeddingProvider.Embed)
            .ToList();

        return Classify(embeddings);
    }

    /// <summary>
    /// Async version that properly awaits embedding calls.
    /// </summary>
    public async Task<IntentInferenceResult> InferAsync(
        BehaviorSpace behaviorSpace,
        BehaviorVector vector,
        CancellationToken cancellationToken = default)
    {
        var tasks = vector.Dimensions.Keys
            .Select(k => _embeddingProvider.EmbedAsync(k, cancellationToken));
        var embeddings = (await Task.WhenAll(tasks)).ToList();

        return Classify(embeddings);
    }

    private IntentInferenceResult Classify(List<IntentEmbedding> embeddings)
    {
        var vectorsWithData = embeddings.Where(e => e.Vector is { Count: > 0 }).ToList();

        string intentName = "Unknown";
        double bestScore = 0;
        string? reasoning = null;

        if (vectorsWithData.Count > 0)
        {
            var avgVector = AverageVectors(vectorsWithData.Select(e => e.Vector!).ToList());
            var match = _catalog.FindBestMatch(avgVector);

            if (match.HasValue)
            {
                intentName = match.Value.Name;
                bestScore = match.Value.Score;
                reasoning = $"Nearest catalog match: {intentName} (similarity: {bestScore:F3})";
            }
        }

        if (bestScore == 0 && embeddings.Count > 0)
        {
            bestScore = embeddings.Average(e => e.Score);
            reasoning = "No catalog match found; using average embedding score";
        }

        var signals = embeddings.Select(e =>
            new IntentSignal(Source: "catalog", Description: e.Source, Weight: e.Score))
            .ToList();

        return new IntentInferenceResult(
            Name: intentName,
            Score: bestScore,
            Signals: signals,
            Reasoning: reasoning
        );
    }

    private static IReadOnlyList<double> AverageVectors(List<IReadOnlyList<double>> vectors)
    {
        var dim = vectors[0].Count;
        var avg = new double[dim];

        foreach (var vec in vectors)
        {
            for (var i = 0; i < Math.Min(dim, vec.Count); i++)
                avg[i] += vec[i];
        }

        for (var i = 0; i < dim; i++)
            avg[i] /= vectors.Count;

        return avg;
    }
}
