using System.Net;
using System.Text;
using Intentum.AI.AzureOpenAI;
using Intentum.AI.Gemini;
using Intentum.AI.Mistral;
using Intentum.AI.OpenAI;

namespace Intentum.Tests;

public class ProviderHttpTests
{
    [Fact]
    public void OpenAIEmbeddingProvider_ParsesEmbeddingScore()
    {
        var json = """
        {
          "data": [
            { "embedding": [0.5, -0.5] }
          ]
        }
        """;

        var provider = new OpenAIEmbeddingProvider(
            new OpenAIOptions { ApiKey = "k", EmbeddingModel = "text-embedding-3-large", BaseUrl = "https://api.openai.com/v1/" },
            CreateClient(json));

        var result = provider.Embed("user:login");

        Assert.InRange(result.Score, 0.49, 0.51);
    }

    [Fact]
    public void GeminiEmbeddingProvider_ParsesEmbeddingScore()
    {
        var json = """
        { "embedding": { "values": [0.2, -0.2] } }
        """;

        var provider = new GeminiEmbeddingProvider(
            new GeminiOptions { ApiKey = "k", EmbeddingModel = "text-embedding-004", BaseUrl = "https://generativelanguage.googleapis.com/v1beta/" },
            CreateClient(json));

        var result = provider.Embed("user:retry");

        Assert.InRange(result.Score, 0.19, 0.21);
    }

    [Fact]
    public void MistralEmbeddingProvider_ParsesEmbeddingScore()
    {
        var json = """
        { "data": [ { "embedding": [0.3, -0.3] } ] }
        """;

        var provider = new MistralEmbeddingProvider(
            new MistralOptions { ApiKey = "k", EmbeddingModel = "mistral-embed", BaseUrl = "https://api.mistral.ai/v1/" },
            CreateClient(json));

        var result = provider.Embed("user:submit");

        Assert.InRange(result.Score, 0.29, 0.31);
    }

    [Fact]
    public void AzureOpenAIEmbeddingProvider_ParsesEmbeddingScore()
    {
        var json = """
        { "data": [ { "embedding": [0.4, -0.4] } ] }
        """;

        var provider = new AzureOpenAIEmbeddingProvider(
            new AzureOpenAIOptions
            {
                Endpoint = "https://example.openai.azure.com/",
                ApiKey = "k",
                EmbeddingDeployment = "embedding",
                ApiVersion = "2023-05-15"
            },
            CreateClient(json));

        var result = provider.Embed("system:challenge");

        Assert.InRange(result.Score, 0.39, 0.41);
    }

    [Fact]
    public void OpenAIEmbeddingProvider_ThrowsOnBadStatus()
    {
        var provider = new OpenAIEmbeddingProvider(
            new OpenAIOptions { ApiKey = "k", EmbeddingModel = "text-embedding-3-large", BaseUrl = "https://api.openai.com/v1/" },
            CreateClient("", HttpStatusCode.BadRequest));

        Assert.Throws<HttpRequestException>(() => provider.Embed("user:login"));
    }

    private static HttpClient CreateClient(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new FakeHandler(_ =>
            new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
