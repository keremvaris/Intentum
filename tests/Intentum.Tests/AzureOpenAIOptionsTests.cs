using Intentum.AI.AzureOpenAI;

namespace Intentum.Tests;

public sealed class AzureOpenAIOptionsTests
{
    [Fact]
    public void Validate_WhenValid_DoesNotThrow()
    {
        var options = new AzureOpenAIOptions
        {
            Endpoint = "https://my.openai.azure.com/",
            ApiKey = "key",
            EmbeddingDeployment = "embedding",
            ApiVersion = "2023-05-15"
        };
        var ex = Record.Exception(() => options.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_WhenEndpointEmpty_Throws()
    {
        var options = new AzureOpenAIOptions
        {
            Endpoint = "",
            ApiKey = "k",
            EmbeddingDeployment = "e",
            ApiVersion = "v"
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("Endpoint", ex.Message);
    }

    [Fact]
    public void Validate_WhenApiKeyEmpty_Throws()
    {
        var options = new AzureOpenAIOptions
        {
            Endpoint = "https://x/",
            ApiKey = "",
            EmbeddingDeployment = "e",
            ApiVersion = "v"
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("ApiKey", ex.Message);
    }

    [Fact]
    public void Validate_WhenEmbeddingDeploymentEmpty_Throws()
    {
        var options = new AzureOpenAIOptions
        {
            Endpoint = "https://x/",
            ApiKey = "k",
            EmbeddingDeployment = "",
            ApiVersion = "v"
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("EmbeddingDeployment", ex.Message);
    }

    [Fact]
    public void Validate_WhenApiVersionEmpty_Throws()
    {
        var options = new AzureOpenAIOptions
        {
            Endpoint = "https://x/",
            ApiKey = "k",
            EmbeddingDeployment = "e",
            ApiVersion = ""
        };
        var ex = Assert.Throws<ArgumentException>(() => options.Validate());
        Assert.Contains("ApiVersion", ex.Message);
    }
}
