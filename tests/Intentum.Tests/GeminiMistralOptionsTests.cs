using Intentum.AI.Gemini;
using Intentum.AI.Mistral;

namespace Intentum.Tests;

public sealed class GeminiMistralOptionsTests
{
    [Fact]
    public void GeminiOptions_Validate_WhenValid_DoesNotThrow()
    {
        var options = new GeminiOptions
        {
            ApiKey = "k",
            EmbeddingModel = "text-embedding-004",
            BaseUrl = "https://generativelanguage.googleapis.com/"
        };
        var ex = Record.Exception(() => options.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void GeminiOptions_Validate_WhenApiKeyEmpty_Throws()
    {
        var options = new GeminiOptions
        {
            ApiKey = "",
            EmbeddingModel = "m",
            BaseUrl = "https://x/"
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("ApiKey", ex.Message);
    }

    [Fact]
    public void GeminiOptions_Validate_WhenEmbeddingModelEmpty_Throws()
    {
        var options = new GeminiOptions
        {
            ApiKey = "k",
            EmbeddingModel = "",
            BaseUrl = "https://x/"
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("EmbeddingModel", ex.Message);
    }

    [Fact]
    public void GeminiOptions_Validate_WhenBaseUrlEmpty_Throws()
    {
        var options = new GeminiOptions
        {
            ApiKey = "k",
            EmbeddingModel = "m",
            BaseUrl = ""
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("BaseUrl", ex.Message);
    }

    [Fact]
    public void MistralOptions_Validate_WhenValid_DoesNotThrow()
    {
        var options = new MistralOptions
        {
            ApiKey = "k",
            EmbeddingModel = "mistral-embed",
            BaseUrl = "https://api.mistral.ai/"
        };
        var ex = Record.Exception(() => options.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void MistralOptions_Validate_WhenApiKeyEmpty_Throws()
    {
        var options = new MistralOptions
        {
            ApiKey = "",
            EmbeddingModel = "m",
            BaseUrl = "https://x/"
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("ApiKey", ex.Message);
    }

    [Fact]
    public void MistralOptions_Validate_WhenBaseUrlEmpty_Throws()
    {
        var options = new MistralOptions
        {
            ApiKey = "k",
            EmbeddingModel = "m",
            BaseUrl = ""
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("BaseUrl", ex.Message);
    }

    [Fact]
    public void MistralOptions_Validate_WhenEmbeddingModelEmpty_Throws()
    {
        var options = new MistralOptions
        {
            ApiKey = "k",
            EmbeddingModel = "",
            BaseUrl = "https://x/"
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("EmbeddingModel", ex.Message);
    }
}
