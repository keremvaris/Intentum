using Intentum.AI.Embeddings;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

namespace Intentum.AI.OpenAI;

public sealed class OpenAIIntentModel(
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine)
    : ProviderLlmIntentModelBase("openai", embeddingProvider, similarityEngine);
