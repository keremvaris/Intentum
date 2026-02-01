using System.Net;
using Intentum.AI.Models;
using Intentum.AI.OpenAI;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Real OpenAI API integration tests. Run when OPENAI_API_KEY is set (e.g. via .env or scripts/run-integration-tests.sh).
/// Skipped when the key is missing so the full suite can pass without OpenAI credentials.
/// </summary>
[Trait("Category", "Integration")]
public class OpenAIIntegrationTests
{
    private static bool HasRealApiKey => !string.IsNullOrWhiteSpace(
        Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

    private const string MissingKeyMessage =
        "OPENAI_API_KEY is not set. Copy .env.example to .env and set OPENAI_API_KEY (or run ./scripts/run-integration-tests.sh which loads .env). " +
        "To exclude these tests: --filter \"FullyQualifiedName!=Intentum.Tests.OpenAIIntegrationTests\".";

    [SkippableFact]
    public void OpenAIEmbeddingProvider_RealApi_ReturnsValidScore()
    {
        Skip.If(!HasRealApiKey, MissingKeyMessage);

        try
        {
            var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL")
                ?? "https://api.openai.com/v1/";
            var options = new OpenAIOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
                EmbeddingModel = Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL") ?? "text-embedding-3-small",
                BaseUrl = baseUrl.TrimEnd('/') + "/"
            };
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.BaseUrl!);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + options.ApiKey);

            var provider = new OpenAIEmbeddingProvider(options, httpClient);
            var result = provider.Embed("user:login");

            Assert.NotNull(result);
            Assert.Equal("user:login", result.Source);
            Assert.InRange(result.Score, 0.0, 1.0);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Skip: free tier rate limit; run again later or use paid tier
        }
    }

    [SkippableFact]
    public void LlmIntentModel_RealOpenAI_FullPipeline_ProducesIntent()
    {
        Skip.If(!HasRealApiKey, MissingKeyMessage);

        try
        {
            var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL")
                ?? "https://api.openai.com/v1/";
            var options = new OpenAIOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
                EmbeddingModel = Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL") ?? "text-embedding-3-small",
                BaseUrl = baseUrl.TrimEnd('/') + "/"
            };
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.BaseUrl!);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + options.ApiKey);

            var embeddingProvider = new OpenAIEmbeddingProvider(options, httpClient);
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
            // Skip: free tier rate limit; run again later or use paid tier
        }
    }
}
