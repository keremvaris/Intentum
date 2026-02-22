using Intentum.AI.Catalog;
using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Tests for CatalogIntentModel: Infer with mock catalog and provider, null validation.
/// </summary>
public sealed class CatalogIntentModelTests
{
    [Fact]
    public void Constructor_WithNullEmbeddingProvider_ThrowsArgumentNullException()
    {
        var catalog = new IntentCatalog().Define("A", "Desc", "key");

        Assert.Throws<ArgumentNullException>(() =>
            new CatalogIntentModel(null!, catalog));
    }

    [Fact]
    public void Constructor_WithNullCatalog_ThrowsArgumentNullException()
    {
        var provider = new MockEmbeddingProvider();

        Assert.Throws<ArgumentNullException>(() =>
            new CatalogIntentModel(provider, null!));
    }

    [Fact]
    public async Task Infer_WithResolvedCatalog_ReturnsIntentFromCatalog()
    {
        var catalog = new IntentCatalog()
            .Define("Login", "Login intent", "user:login")
            .Define("Submit", "Submit intent", "user:submit");
        var provider = new MockEmbeddingProvider();
        await catalog.ResolveEmbeddingsAsync(provider);
        var model = new CatalogIntentModel(provider, catalog);
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("login")
            .Action("submit")
            .Build();

        var intent = model.Infer(space);

        Assert.NotNull(intent);
        Assert.True(intent.Name is "Login" or "Submit");
        Assert.InRange(intent.Confidence.Score, 0.0, 1.0);
    }

    [Fact]
    public async Task Infer_WithPrecomputedVector_UsesVectorWithoutRecomputing()
    {
        var catalog = new IntentCatalog()
            .Add(new IntentDefinition("Single", "One intent", [], new List<double> { 1.0, 0.0, 0.0 }));
        var provider = new MockEmbeddingProvider();
        await catalog.ResolveEmbeddingsAsync(provider);
        var model = new CatalogIntentModel(provider, catalog);
        var space = new BehaviorSpaceBuilder().WithActor("user").Action("login").Build();
        var precomputed = new BehaviorVector(new Dictionary<string, double> { ["user:login"] = 1 });

        var intent = model.Infer(space, precomputed);

        Assert.NotNull(intent);
        Assert.Equal("Single", intent.Name);
    }
}
