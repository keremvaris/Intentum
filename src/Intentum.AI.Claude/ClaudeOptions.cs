namespace Intentum.AI.Claude;

public sealed class ClaudeOptions
{
    public const string DefaultBaseUrl = "https://api.anthropic.com/v1/";

    public required string ApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "claude-embedding-1";
    public string Model { get; init; } = "claude-3-5-sonnet-20240620";
    public string BaseUrl { get; init; } = DefaultBaseUrl;
    public string ApiVersion { get; init; } = "2023-06-01";
    public bool UseMessagesScoring { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("Claude ApiKey is required.", nameof(ApiKey));
        if (string.IsNullOrWhiteSpace(Model))
            throw new ArgumentException("Claude Model is required.", nameof(Model));
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("Claude BaseUrl is required.", nameof(BaseUrl));
        if (string.IsNullOrWhiteSpace(ApiVersion))
            throw new ArgumentException("Claude ApiVersion is required.", nameof(ApiVersion));
    }

    public static ClaudeOptions FromEnvironment()
    {
        var apiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY")
            ?? throw new InvalidOperationException("CLAUDE_API_KEY is not set.");

        return new ClaudeOptions
        {
            ApiKey = apiKey,
            Model = Environment.GetEnvironmentVariable("CLAUDE_MODEL") ?? "claude-3-5-sonnet-20240620",
            BaseUrl = Environment.GetEnvironmentVariable("CLAUDE_BASE_URL") ?? DefaultBaseUrl,
            ApiVersion = Environment.GetEnvironmentVariable("CLAUDE_API_VERSION") ?? "2023-06-01",
            UseMessagesScoring = (Environment.GetEnvironmentVariable("CLAUDE_USE_MESSAGES_SCORING") ?? "false")
                .Equals("true", StringComparison.OrdinalIgnoreCase)
        };
    }
}
