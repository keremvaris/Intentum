using Intentum.AI.Embeddings;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

namespace Intentum.AI.Gemini;

public sealed class GeminiIntentModel(
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine)
    : ProviderLlmIntentModelBase("gemini", embeddingProvider, similarityEngine);
