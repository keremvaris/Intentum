using System.Net;
using Intentum.AI.Models;
using Intentum.AI.Mistral;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Real Mistral API integration tests. Run when MISTRAL_API_KEY is set (e.g. via .env or scripts/run-mistral-integration-tests.sh).
/// Fails with a clear message when the key is missing.
/// </summary>
[Trait("Category", "Integration")]
public class MistralIntegrationTests
{
    private static bool HasRealApiKey => !string.IsNullOrWhiteSpace(
        Environment.GetEnvironmentVariable("MISTRAL_API_KEY"));

    private const string MissingKeyMessage =
        "MISTRAL_API_KEY is not set. Copy .env.example to .env and set MISTRAL_API_KEY (and optionally MISTRAL_BASE_URL). " +
        "Run: ./scripts/run-mistral-integration-tests.sh. To exclude: --filter \"FullyQualifiedName!=Intentum.Tests.MistralIntegrationTests\".";

    [Fact]
    public void MistralEmbeddingProvider_RealApi_ReturnsValidScore()
    {
        Assert.True(HasRealApiKey, MissingKeyMessage);

        try
        {
            var baseUrl = Environment.GetEnvironmentVariable("MISTRAL_BASE_URL")
                ?? "https://api.mistral.ai/v1/";
            var options = new MistralOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY")!,
                EmbeddingModel = Environment.GetEnvironmentVariable("MISTRAL_EMBEDDING_MODEL") ?? "mistral-embed",
                BaseUrl = baseUrl.TrimEnd('/') + "/"
            };
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.BaseUrl!);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + options.ApiKey);

            var provider = new MistralEmbeddingProvider(options, httpClient);
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

    [Fact]
    public void LlmIntentModel_RealMistral_FullPipeline_ProducesIntent()
    {
        Assert.True(HasRealApiKey, MissingKeyMessage);

        try
        {
            var baseUrl = Environment.GetEnvironmentVariable("MISTRAL_BASE_URL")
                ?? "https://api.mistral.ai/v1/";
            var options = new MistralOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY")!,
                EmbeddingModel = Environment.GetEnvironmentVariable("MISTRAL_EMBEDDING_MODEL") ?? "mistral-embed",
                BaseUrl = baseUrl.TrimEnd('/') + "/"
            };
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.BaseUrl!);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + options.ApiKey);

            var embeddingProvider = new MistralEmbeddingProvider(options, httpClient);
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
