using Intentum.AI.Catalog;
using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Moq;

namespace Intentum.Tests;

/// <summary>
/// Tests for IntentCatalog: Add, Define, ResolveEmbeddingsAsync, FindBestMatch, GetEmbedding.
/// </summary>
public sealed class IntentCatalogTests
{
    [Fact]
    public void Add_WithValidDefinition_AddsToDefinitions()
    {
        var catalog = new IntentCatalog();
        var def = new IntentDefinition("Login", "User login intent", ["user:login"]);

        catalog.Add(def);

        Assert.Single(catalog.Definitions);
        Assert.Equal("Login", catalog.Definitions[0].Name);
        Assert.Equal("User login intent", catalog.Definitions[0].Description);
    }

    [Fact]
    public void Add_WithNull_ThrowsArgumentNullException()
    {
        var catalog = new IntentCatalog();

        Assert.Throws<ArgumentNullException>(() => catalog.Add(null!));
    }

    [Fact]
    public void Define_AddsDefinitionWithExampleKeys()
    {
        var catalog = new IntentCatalog();

        catalog.Define("Checkout", "Purchase intent", "user:add_to_cart", "user:checkout");

        Assert.Single(catalog.Definitions);
        Assert.Equal("Checkout", catalog.Definitions[0].Name);
        Assert.Equal(2, catalog.Definitions[0].ExampleBehaviorKeys.Count);
        Assert.Contains("user:add_to_cart", catalog.Definitions[0].ExampleBehaviorKeys);
    }

    [Fact]
    public void Define_ReturnsCatalogForChaining()
    {
        var catalog = new IntentCatalog();

        var result = catalog.Define("A", "Desc", "key1").Define("B", "Desc2");

        Assert.Equal(2, catalog.Definitions.Count);
        Assert.Same(catalog, result);
    }

    [Fact]
    public async Task ResolveEmbeddingsAsync_WithMockProvider_ResolvesEmbeddings()
    {
        var catalog = new IntentCatalog()
            .Define("Login", "Login intent", "user:login")
            .Define("Submit", "Submit intent", "user:submit");
        var provider = new MockEmbeddingProvider();

        await catalog.ResolveEmbeddingsAsync(provider);

        Assert.NotNull(catalog.GetEmbedding("Login"));
        Assert.NotNull(catalog.GetEmbedding("Submit"));
    }

    [Fact]
    public async Task ResolveEmbeddingsAsync_WithPrecomputedReferenceEmbedding_SkipsProvider()
    {
        var precomputed = new List<double> { 0.5, 0.5 };
        var catalog = new IntentCatalog()
            .Add(new IntentDefinition("Precomputed", "Has embedding", [], precomputed));
        var mockProvider = new Mock<IIntentEmbeddingProvider>();
        mockProvider.Setup(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntentEmbedding("x", 0, []));

        await catalog.ResolveEmbeddingsAsync(mockProvider.Object);

        mockProvider.Verify(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(precomputed, catalog.GetEmbedding("Precomputed"));
    }

    [Fact]
    public async Task ResolveEmbeddingsAsync_WithEmptyExampleKeys_SkipsDefinition()
    {
        var catalog = new IntentCatalog()
            .Define("Empty", "No examples", []);
        var mockProvider = new Mock<IIntentEmbeddingProvider>();

        await catalog.ResolveEmbeddingsAsync(mockProvider.Object);

        mockProvider.Verify(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Null(catalog.GetEmbedding("Empty"));
    }

    [Fact]
    public void GetEmbedding_WhenNotResolved_ReturnsNull()
    {
        var catalog = new IntentCatalog().Define("X", "Desc", "key");

        Assert.Null(catalog.GetEmbedding("X"));
        Assert.Null(catalog.GetEmbedding("Nonexistent"));
    }

    [Fact]
    public async Task FindBestMatch_WithEmptyEmbedding_ReturnsNull()
    {
        var catalog = new IntentCatalog().Define("A", "Desc", "key");
        var provider = new MockEmbeddingProvider();
        await catalog.ResolveEmbeddingsAsync(provider);

        var result = catalog.FindBestMatch([]);

        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_WithEmptyCatalog_ReturnsNull()
    {
        var catalog = new IntentCatalog();
        var embedding = new List<double> { 0.5, 0.5 };

        var result = catalog.FindBestMatch(embedding);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindBestMatch_WithResolvedCatalog_ReturnsBestMatch()
    {
        var catalog = new IntentCatalog()
            .Define("Login", "Login", "user:login")
            .Define("Submit", "Submit", "user:submit");
        var provider = new MockEmbeddingProvider();
        await catalog.ResolveEmbeddingsAsync(provider);

        var behaviorEmbedding = provider.Embed("user:login").Vector ?? [];
        var result = catalog.FindBestMatch(behaviorEmbedding);

        Assert.NotNull(result);
        Assert.True(result.Value.Score >= 0 && result.Value.Score <= 1);
    }

    [Fact]
    public async Task FindBestMatch_IsCaseInsensitive()
    {
        var catalog = new IntentCatalog().Define("Login", "Desc", "user:login");
        var provider = new MockEmbeddingProvider();
        await catalog.ResolveEmbeddingsAsync(provider);
        var embedding = catalog.GetEmbedding("Login")!;

        var result = catalog.FindBestMatch(embedding);

        Assert.NotNull(result);
        Assert.Equal("Login", result.Value.Name);
    }
}
