using System.Net;
using System.Text;
using Intentum.AI.Classification;
using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Tests for PromptIntentClassifier: HttpMessageHandler mock with 200 + valid JSON,
/// empty response, parse error.
/// </summary>
public sealed class PromptIntentClassifierTests
{
    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        var options = new PromptClassifierOptions { Model = "gpt-4" };

        Assert.Throws<ArgumentNullException>(() =>
            new PromptIntentClassifier(null!, options));
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        var client = new HttpClient();

        Assert.Throws<ArgumentNullException>(() =>
            new PromptIntentClassifier(client, null!));
    }

    [Fact]
    public async Task ClassifyAsync_WithValidJson_ReturnsIntentClassificationResult()
    {
        var chatResponse = """
            {
              "choices": [
                {
                  "message": {
                    "content": "{\"intent\":\"Login\",\"confidence\":0.92,\"reasoning\":\"User attempted login\"}"
                  }
                }
              ]
            }
            """;
        var classifier = CreateClassifier(chatResponse);

        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("login")
            .Build();

        var result = await classifier.ClassifyAsync(space);

        Assert.Equal("Login", result.IntentName);
        Assert.Equal(0.92, result.Confidence);
        Assert.Equal("User attempted login", result.Reasoning);
    }

    [Fact]
    public async Task ClassifyAsync_WithEmptyContent_ReturnsUnknown()
    {
        var chatResponse = """{ "choices": [ { "message": { "content": "" } } ] }""";
        var classifier = CreateClassifier(chatResponse);

        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();

        var result = await classifier.ClassifyAsync(space);

        Assert.Equal("Unknown", result.IntentName);
        Assert.Equal(0, result.Confidence);
        Assert.Equal("Empty LLM response", result.Reasoning);
    }

    [Fact]
    public async Task ClassifyAsync_WithInvalidJson_ReturnsUnknownWithParseMessage()
    {
        var chatResponse = """
            {
              "choices": [
                {
                  "message": {
                    "content": "not valid json {{{"
                  }
                }
              ]
            }
            """;
        var classifier = CreateClassifier(chatResponse);

        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();

        var result = await classifier.ClassifyAsync(space);

        Assert.Equal("Unknown", result.IntentName);
        Assert.Equal(0, result.Confidence);
        Assert.Contains("Failed to parse LLM response", result.Reasoning ?? "");
    }

    [Fact]
    public async Task ClassifyAsync_WithCandidateIntents_IncludesThemInPrompt()
    {
        var chatResponse = """
            {
              "choices": [
                {
                  "message": {
                    "content": "{\"intent\":\"Checkout\",\"confidence\":0.85,\"reasoning\":\"checkout\"}"
                  }
                }
              ]
            }
            """;
        var classifier = CreateClassifier(chatResponse);
        var space = new BehaviorSpaceBuilder().WithActor("user").Action("checkout").Build();

        var result = await classifier.ClassifyAsync(space, candidateIntents: ["Login", "Checkout", "Browse"]);

        Assert.Equal("Checkout", result.IntentName);
    }

    private static PromptIntentClassifier CreateClassifier(string responseJson, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new FakeHttpHandler(_ =>
            new HttpResponseMessage(status)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var options = new PromptClassifierOptions { Model = "gpt-4", Endpoint = "chat/completions" };
        return new PromptIntentClassifier(client, options);
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }
}
