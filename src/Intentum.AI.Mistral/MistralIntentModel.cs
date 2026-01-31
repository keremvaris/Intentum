using Intentum.AI.Embeddings;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

namespace Intentum.AI.Mistral;

public sealed class MistralIntentModel(
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine)
    : ProviderLlmIntentModelBase("mistral", embeddingProvider, similarityEngine);
