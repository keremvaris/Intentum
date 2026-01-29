using Intentum.AI.AzureOpenAI;
using Intentum.AI.Embeddings;
using Intentum.AI.Gemini;
using Intentum.AI.Mock;
using Intentum.AI.Mistral;
using Intentum.AI.OpenAI;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Tests;

public sealed class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddIntentumOpenAI_RegistersServices()
    {
        var options = new OpenAIOptions
        {
            ApiKey = "sk-test",
            EmbeddingModel = "text-embedding-3-small",
            BaseUrl = "https://api.openai.com/v1/"
        };
        var services = new ServiceCollection();
        services.AddIntentumOpenAI(options);
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<OpenAIOptions>());
        Assert.NotNull(provider.GetService<HttpClient>());
        Assert.NotNull(provider.GetService<IIntentEmbeddingProvider>());
        Assert.NotNull(provider.GetService<IIntentSimilarityEngine>());
        Assert.NotNull(provider.GetService<IIntentModel>());
    }

    [Fact]
    public void OpenAIIntentModel_Infer_WithMockProvider_ReturnsIntent()
    {
        var model = new OpenAIIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("login")
            .Action("submit")
            .Build();
        var intent = model.Infer(space);
        Assert.NotNull(intent);
        Assert.Equal("AI-Inferred-Intent", intent.Name);
        Assert.NotEmpty(intent.Signals);
        Assert.InRange(intent.Confidence.Score, 0.0, 1.0);
    }

    [Fact]
    public void GeminiIntentModel_Infer_WithMockProvider_ReturnsIntent()
    {
        var model = new GeminiIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("login")
            .Build();
        var intent = model.Infer(space);
        Assert.NotNull(intent);
        Assert.Equal("AI-Inferred-Intent", intent.Name);
        Assert.NotEmpty(intent.Signals);
    }

    [Fact]
    public void MistralIntentModel_Infer_WithMockProvider_ReturnsIntent()
    {
        var model = new MistralIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("submit")
            .Build();
        var intent = model.Infer(space);
        Assert.NotNull(intent);
        Assert.Equal("AI-Inferred-Intent", intent.Name);
        Assert.NotEmpty(intent.Signals);
    }

    [Fact]
    public void AzureOpenAIIntentModel_Infer_WithMockProvider_ReturnsIntent()
    {
        var model = new AzureOpenAIIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("login")
            .Action("submit")
            .Build();
        var intent = model.Infer(space);
        Assert.NotNull(intent);
        Assert.Equal("AI-Inferred-Intent", intent.Name);
        Assert.NotEmpty(intent.Signals);
        Assert.InRange(intent.Confidence.Score, 0.0, 1.0);
    }
}
