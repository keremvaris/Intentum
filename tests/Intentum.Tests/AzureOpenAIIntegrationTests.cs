using System.Net;
using Intentum.AI.Models;
using Intentum.AI.AzureOpenAI;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Real Azure OpenAI API integration tests. Run when AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY are set (e.g. via .env or scripts/run-azure-integration-tests.sh).
/// Skipped when keys are missing so the full suite can pass without Azure credentials.
/// </summary>
[Trait("Category", "Integration")]
public class AzureOpenAIIntegrationTests
{
    private static bool HasKeys =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")) &&
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));

    private const string MissingKeyMessage =
        "AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY are not set. Copy .env.example to .env and set them. " +
        "Run: ./scripts/run-azure-integration-tests.sh. To exclude: --filter \"FullyQualifiedName!=Intentum.Tests.AzureOpenAIIntegrationTests\".";

    [SkippableFact]
    public void AzureOpenAIEmbeddingProvider_RealApi_ReturnsValidScore()
    {
        Skip.If(!HasKeys, MissingKeyMessage);

        try
        {
            var options = new AzureOpenAIOptions
            {
                Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!.TrimEnd('/') + "/",
                ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!,
                EmbeddingDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT") ?? "embedding",
                ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2023-05-15"
            };
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.Endpoint);
            httpClient.DefaultRequestHeaders.Add("api-key", options.ApiKey);

            var provider = new AzureOpenAIEmbeddingProvider(options, httpClient);
            var result = provider.Embed("user:login");

            Assert.NotNull(result);
            Assert.Equal("user:login", result.Source);
            Assert.InRange(result.Score, 0.0, 1.0);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Skip: rate limit; run again later
        }
    }

    [SkippableFact]
    public void LlmIntentModel_RealAzureOpenAI_FullPipeline_ProducesIntent()
    {
        Skip.If(!HasKeys, MissingKeyMessage);

        try
        {
            var options = new AzureOpenAIOptions
            {
                Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!.TrimEnd('/') + "/",
                ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!,
                EmbeddingDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT") ?? "embedding",
                ApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2023-05-15"
            };
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.Endpoint);
            httpClient.DefaultRequestHeaders.Add("api-key", options.ApiKey);

            var embeddingProvider = new AzureOpenAIEmbeddingProvider(options, httpClient);
            var similarityEngine = new SimpleAverageSimilarityEngine();
            var model = new LlmIntentModel(embeddingProvider, similarityEngine);

            var space = new BehaviorSpace()
                .Observe("user", "login.attempt")
                .Observe("user", "login.success");

            var intent = model.Infer(space);

            Assert.NotNull(intent);
            Assert.Equal("AI-Inferred-Intent", intent.Name);
            Assert.NotNull(intent.Confidence);
            Assert.InRange(intent.Confidence.Score, 0.0, 1.0);
            Assert.NotEmpty(intent.Signals);

            var policy = new IntentPolicy()
                .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
                .AddRule(new PolicyRule("Observe", _ => true, PolicyDecision.Observe));
            var decision = intent.Decide(policy);
            Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Skip: rate limit; run again later
        }
    }
}
