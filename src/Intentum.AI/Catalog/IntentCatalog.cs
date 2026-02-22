using Intentum.AI.Embeddings;

namespace Intentum.AI.Catalog;

/// <summary>
/// Registry of known intent definitions with their reference embeddings.
/// Used by CatalogIntentClassifier to perform nearest-neighbor intent classification.
/// </summary>
public sealed class IntentCatalog
{
    private readonly List<IntentDefinition> _definitions = [];
    private readonly Dictionary<string, IReadOnlyList<double>> _resolvedEmbeddings = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<IntentDefinition> Definitions => _definitions;

    /// <summary>
    /// Adds an intent definition to the catalog.
    /// </summary>
    public IntentCatalog Add(IntentDefinition definition)
    {
        _definitions.Add(definition ?? throw new ArgumentNullException(nameof(definition)));
        return this;
    }

    /// <summary>
    /// Adds an intent definition using a fluent builder pattern.
    /// </summary>
    public IntentCatalog Define(string name, string description, params string[] exampleBehaviorKeys)
    {
        _definitions.Add(new IntentDefinition(name, description, exampleBehaviorKeys));
        return this;
    }

    /// <summary>
    /// Resolves reference embeddings for all definitions using the given provider.
    /// For each definition, the embedding is computed by averaging the embeddings of all example behavior keys.
    /// </summary>
    public async Task ResolveEmbeddingsAsync(
        IIntentEmbeddingProvider provider,
        CancellationToken cancellationToken = default)
    {
        _resolvedEmbeddings.Clear();

        foreach (var definition in _definitions)
        {
            if (definition.ReferenceEmbedding is { Count: > 0 })
            {
                _resolvedEmbeddings[definition.Name] = definition.ReferenceEmbedding;
                continue;
            }

            if (definition.ExampleBehaviorKeys.Count == 0)
                continue;

            var embeddings = new List<IReadOnlyList<double>>();
            foreach (var key in definition.ExampleBehaviorKeys)
            {
                var result = await provider.EmbedAsync(key, cancellationToken);
                if (result.Vector is { Count: > 0 })
                    embeddings.Add(result.Vector);
            }

            if (embeddings.Count > 0)
                _resolvedEmbeddings[definition.Name] = AverageVectors(embeddings);
        }
    }

    /// <summary>
    /// Gets the resolved embedding for a definition by name, or null if not resolved.
    /// </summary>
    public IReadOnlyList<double>? GetEmbedding(string intentName)
        => _resolvedEmbeddings.TryGetValue(intentName, out var embedding) ? embedding : null;

    /// <summary>
    /// Finds the best matching intent by comparing a behavior embedding against all catalog entries.
    /// Returns the best match name and cosine similarity score, or null if no match found.
    /// </summary>
    public (string Name, double Score)? FindBestMatch(IReadOnlyList<double> behaviorEmbedding)
    {
        if (behaviorEmbedding.Count == 0 || _resolvedEmbeddings.Count == 0)
            return null;

        string? bestName = null;
        var bestScore = -1.0;

        foreach (var (name, refEmbedding) in _resolvedEmbeddings)
        {
            var score = EmbeddingScore.CosineSimilarity(behaviorEmbedding, refEmbedding);
            if (score > bestScore)
            {
                bestScore = score;
                bestName = name;
            }
        }

        return bestName != null ? (bestName, bestScore) : null;
    }

    private static double[] AverageVectors(List<IReadOnlyList<double>> vectors)
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
