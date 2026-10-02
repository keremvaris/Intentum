using System.Net;
using System.Text;
using Intentum.AI.SystemOne;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Tests;

/// <summary>
/// Tests for the System One (Jev-compatible /v1/systemone) HTTP adapter:
/// client request/response round-trip, BehaviorSpace → Intent mapping,
/// engine presets (Kev, Laya, TinyJev, ...) and DI registration.
/// </summary>
public sealed class SystemOneTests
{
    [Fact]
    public void Decide_PostsStateAndQuestions_AndParsesAllAnswerTypes()
    {
        string? requestBody = null;
        var handler = new FakeHandler(request =>
        {
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return Response("""
                {
                  "answers": {
                    "intent": {"choice": "billing", "probabilities": {"billing": 0.9, "technical": 0.1}, "confidence": 0.87},
                    "urgent": {"noul": 0.8},
                    "mood": {"score": 1.4, "confidence": 0.6}
                  }
                }
                """);
        });
        var client = new SystemOneClient(
            new SystemOneOptions { BaseUrl = "http://127.0.0.1:8009" },
            new HttpClient(handler));

        var questions = new Dictionary<string, SystemOneQuestion>
        {
            ["intent"] = SystemOneQuestion.Choice(
                "Which team should handle this?",
                new Dictionary<string, string?> { ["billing"] = "refunds", ["technical"] = "bugs" }),
            ["urgent"] = SystemOneQuestion.Noul("Is this urgent?"),
            ["mood"] = SystemOneQuestion.Score("How frustrated is the user?", ["calm", "frustrated", "angry"]),
        };

        var response = client.Decide("user was charged twice", questions);

        Assert.NotNull(requestBody);
        Assert.Contains("\"state\":\"user was charged twice\"", requestBody);
        Assert.Contains("\"questions\"", requestBody);
        Assert.Contains("\"type\":\"choice\"", requestBody);
        Assert.Contains("\"type\":\"noul\"", requestBody);
        Assert.Contains("\"type\":\"score\"", requestBody);
        Assert.Contains("\"criteria\":[\"calm\",\"frustrated\",\"angry\"]", requestBody);

        var intent = response.Answers["intent"];
        Assert.Equal("billing", intent.Choice);
        Assert.Equal(0.9, intent.Probabilities!["billing"]);
        Assert.Equal(0.87, intent.Confidence);
        Assert.Equal(0.8, response.Answers["urgent"].Noul);
        Assert.Equal(1.4, response.Answers["mood"].Score);
    }

    [Fact]
    public void Decide_WithModelAndApiKey_SendsModelAndBearerToken()
    {
        string? requestBody = null;
        string? authorization = null;
        var handler = new FakeHandler(request =>
        {
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            authorization = request.Headers.Authorization?.ToString();
            return Response("""{"answers":{}}""");
        });
        var client = new SystemOneClient(
            new SystemOneOptions { BaseUrl = "http://127.0.0.1:8009", Model = "kev-latest", ApiKey = "secret" },
            new HttpClient(handler));

        client.Decide("state", new Dictionary<string, SystemOneQuestion>());

        Assert.NotNull(requestBody);
        Assert.Contains("\"model\":\"kev-latest\"", requestBody);
        Assert.Equal("Bearer secret", authorization);
    }

    [Fact]
    public void SystemOneIntentModel_InfersIntentFromChoiceAnswer_WithSignalsAndReasoning()
    {
        string? requestBody = null;
        var handler = new FakeHandler(request =>
        {
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return Response("""
                {"answers":{"intent":{"choice":"billing","probabilities":{"billing":0.9,"technical":0.1},"confidence":0.87}}}
                """);
        });
        var options = new SystemOneOptions
        {
            BaseUrl = "http://127.0.0.1:8000",
            Engine = "laya",
            IntentCatalog = new Dictionary<string, string>
            {
                ["billing"] = "invoices, payments, refunds",
                ["technical"] = "bugs, outages, system errors",
            },
        };
        var model = new SystemOneIntentModel(options, new HttpClient(handler));
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("chargeback")
            .Build();

        var intent = model.Infer(space);

        Assert.Equal("billing", intent.Name);
        Assert.Equal(0.87, intent.Confidence.Score);
        Assert.Equal("Certain", intent.Confidence.Level);
        Assert.Contains(intent.Signals, s => s.Source == "laya" && s.Description == "technical" && s.Weight == 0.1);
        Assert.NotNull(intent.Reasoning);
        Assert.Contains("technical", intent.Reasoning);

        Assert.NotNull(requestBody);
        Assert.Contains("user:chargeback", requestBody);
        Assert.Contains("invoices, payments, refunds", requestBody);
    }

    [Fact]
    public void SystemOneIntentModel_WhenEngineReturnsNoAnswers_ReturnsUnknownIntent()
    {
        var handler = new FakeHandler(_ => Response("""{"answers":{}}"""));
        var options = new SystemOneOptions
        {
            BaseUrl = "http://127.0.0.1:8000",
            IntentCatalog = new Dictionary<string, string> { ["billing"] = "refunds" },
        };
        var model = new SystemOneIntentModel(options, new HttpClient(handler));

        var intent = model.Infer(new BehaviorSpaceBuilder().WithActor("user").Action("login").Build());

        Assert.Equal("unknown", intent.Name);
        Assert.Equal(0, intent.Confidence.Score);
        Assert.Empty(intent.Signals);
    }

    [Fact]
    public void SystemOneIntentModel_WithoutIntentCatalog_Throws()
    {
        var handler = new FakeHandler(_ => Response("""{"answers":{}}"""));
        var model = new SystemOneIntentModel(
            new SystemOneOptions { BaseUrl = "http://127.0.0.1:8000" },
            new HttpClient(handler));

        Assert.Throws<InvalidOperationException>(() =>
            model.Infer(new BehaviorSpaceBuilder().WithActor("user").Action("login").Build()));
    }

    [Fact]
    public void SystemOneIntentModel_WithoutBaseUrl_Throws()
    {
        var handler = new FakeHandler(_ => Response("""{"answers":{}}"""));
        var options = new SystemOneOptions
        {
            IntentCatalog = new Dictionary<string, string> { ["billing"] = "refunds" },
        };
        var model = new SystemOneIntentModel(options, new HttpClient(handler));

        Assert.Throws<InvalidOperationException>(() =>
            model.Infer(new BehaviorSpaceBuilder().WithActor("user").Action("login").Build()));
    }

    [Theory]
    [InlineData("jev")]
    [InlineData("kev")]
    [InlineData("tinyjev")]
    [InlineData("laya")]
    [InlineData("opendecision")]
    [InlineData("decision10")]
    [InlineData("openjev")]
    [InlineData("nanojev")]
    [InlineData("openjevpro")]
    [InlineData("foq")]
    public void FromName_KnownEngine_ReturnsEnginePreset(string engine)
    {
        var options = SystemOneEngines.FromName(engine);

        Assert.Equal(engine, options.Engine);
        Assert.StartsWith("http", options.BaseUrl);
        Assert.Equal("/v1/systemone", options.EndpointPath);
    }

    [Fact]
    public void FromName_UnknownEngine_Throws()
    {
        Assert.Throws<ArgumentException>(() => SystemOneEngines.FromName("does-not-exist"));
    }

    [Fact]
    public void Jev_UsesHostedTypeSafeEndpointAndRequiredModel()
    {
        var options = SystemOneEngines.Jev();

        Assert.Equal("https://api.typesafe.ai", options.BaseUrl);
        Assert.Equal("jev", options.Engine);
        Assert.Equal("jev-latest", options.Model);
        Assert.Equal("/v1/systemone", options.EndpointPath);
    }

    [Fact]
    public void FromName_Demo_ReturnsDemoPreset()
    {
        var options = SystemOneEngines.FromName("demo");

        Assert.Equal("demo", options.Engine);
        Assert.Equal("demo-latest", options.Model);
        Assert.Equal("/v1/systemone", options.EndpointPath);
        Assert.Equal("", options.BaseUrl);
        Assert.Equal("demo", SystemOneEngines.Names[0]);
        Assert.Equal("http://x", SystemOneEngines.FromName("demo", "http://x").BaseUrl);
    }

    [Fact]
    public void AddIntentumSystemOne_RegistersIntentModel()
    {
        var services = new ServiceCollection();
        services.AddIntentumSystemOne(new SystemOneOptions
        {
            BaseUrl = "http://127.0.0.1:8000",
            IntentCatalog = new Dictionary<string, string> { ["billing"] = "refunds" },
        });

        using var provider = services.BuildServiceProvider();

        Assert.IsType<SystemOneIntentModel>(provider.GetRequiredService<IIntentModel>());
    }

    private static HttpResponseMessage Response(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> handle) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(handle(request));
    }
}
