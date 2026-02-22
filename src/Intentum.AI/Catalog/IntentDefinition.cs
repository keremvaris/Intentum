namespace Intentum.AI.Catalog;

/// <summary>
/// A reference intent definition used for nearest-neighbor classification.
/// Contains the intent name, description, example behavior keys, and optional pre-computed embedding.
/// </summary>
public sealed record IntentDefinition(
    string Name,
    string Description,
    IReadOnlyList<string> ExampleBehaviorKeys,
    IReadOnlyList<double>? ReferenceEmbedding = null
);
