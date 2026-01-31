using Intentum.AI.Embeddings;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

namespace Intentum.AI.Claude;

public sealed class ClaudeIntentModel(
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine)
    : ProviderLlmIntentModelBase("claude", embeddingProvider, similarityEngine);
