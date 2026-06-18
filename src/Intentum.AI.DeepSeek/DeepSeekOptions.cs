namespace Intentum.AI.DeepSeek;

public sealed class DeepSeekOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.deepseek.com/v1";
    public string EmbeddingModel { get; set; } = "deepseek-embedding";

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
            BaseUrl = Environment.GetEnvironmentVariable("DEEPSEEK_BASE_URL") ?? "https://api.deepseek.com/v1",
            EmbeddingModel = Environment.GetEnvironmentVariable("DEEPSEEK_EMBEDDING_MODEL") ?? "deepseek-embedding"
        };
    }
}
