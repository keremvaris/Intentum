using Intentum.AI.Embeddings;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

namespace Intentum.AI.AzureOpenAI;

public sealed class AzureOpenAIIntentModel(
    IIntentEmbeddingProvider embeddingProvider,
    IIntentSimilarityEngine similarityEngine)
    : ProviderLlmIntentModelBase("azure-openai", embeddingProvider, similarityEngine);
