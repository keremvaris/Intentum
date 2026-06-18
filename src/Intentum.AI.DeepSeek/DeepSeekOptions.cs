namespace Intentum.AI.DeepSeek;

public sealed class DeepSeekOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = DefaultBaseUrl;
    public string EmbeddingModel { get; set; } = "deepseek-embedding";

    internal const string DefaultBaseUrl = "https://api.deepseek.com/v1";

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException("DeepSeek API key is required. Set DEEPSEEK_API_KEY environment variable.");
    }

    public static DeepSeekOptions FromEnvironment()
    {
        return new DeepSeekOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? string.Empty,
            BaseUrl = Environment.GetEnvironmentVariable("DEEPSEEK_BASE_URL") ?? DefaultBaseUrl,
            EmbeddingModel = Environment.GetEnvironmentVariable("DEEPSEEK_EMBEDDING_MODEL") ?? "deepseek-embedding"
        };
    }
}
