using Intentum.AI.OpenAI;

namespace Intentum.Tests;

public sealed class OpenAIOptionsTests
{
    [Fact]
    public void Validate_WhenValid_DoesNotThrow()
    {
        var options = new OpenAIOptions
        {
            ApiKey = "sk-test",
            EmbeddingModel = "text-embedding-3-small",
            BaseUrl = "https://api.openai.com/v1/"
        };

        var ex = Record.Exception(() => options.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_WhenApiKeyEmpty_Throws()
    {
        var options = new OpenAIOptions
        {
            ApiKey = "",
            EmbeddingModel = "text-embedding-3-large",
            BaseUrl = "https://api.openai.com/"
        };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("ApiKey", ex.Message);
    }

    [Fact]
    public void Validate_WhenApiKeyWhitespace_Throws()
    {
        var options = new OpenAIOptions
        {
            ApiKey = "   ",
            EmbeddingModel = "m",
            BaseUrl = "https://x/"
        };

        Assert.Throws<ArgumentException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WhenEmbeddingModelEmpty_Throws()
    {
        var options = new OpenAIOptions
        {
            ApiKey = "sk-test",
            EmbeddingModel = "",
            BaseUrl = "https://api.openai.com/"
        };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("EmbeddingModel", ex.Message);
    }

    [Fact]
    public void Validate_WhenBaseUrlEmpty_Throws()
    {
        var options = new OpenAIOptions
        {
            ApiKey = "sk-test",
            EmbeddingModel = "text-embedding-3-large",
            BaseUrl = ""
        };

        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("BaseUrl", ex.Message);
    }
}
