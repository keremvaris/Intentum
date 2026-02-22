#pragma warning disable IDE0052 // Positional properties used by JSON serialization
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Intentum.AI.Http;
using Intentum.Core.Behavior;

namespace Intentum.AI.Classification;

/// <summary>
/// LLM prompt-based intent classifier. Sends behavior events to an LLM API
/// and receives structured JSON with intent name, confidence, and reasoning.
/// Supports OpenAI-compatible chat completion APIs.
/// </summary>
public sealed class PromptIntentClassifier : IIntentClassifier
{
    private readonly HttpClient _httpClient;
    private readonly PromptClassifierOptions _options;

    public PromptIntentClassifier(HttpClient httpClient, PromptClassifierOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<IntentClassificationResult> ClassifyAsync(
        BehaviorSpace behaviorSpace,
        IReadOnlyList<string>? candidateIntents = null,
        CancellationToken cancellationToken = default)
    {
        var events = behaviorSpace.Events
            .Select(e => $"  - {e.Actor}:{e.Action}")
            .ToList();

        var candidateList = candidateIntents is { Count: > 0 }
            ? $"\nCandidate intents (pick one): {string.Join(", ", candidateIntents)}"
            : "";

        var systemPrompt = _options.SystemPrompt ?? DefaultSystemPrompt;
        var jsonExample = """{"intent":"...","confidence":0.0-1.0,"reasoning":"..."}""";
        var userPrompt = $"""
            Behavior events observed:
            {string.Join("\n", events)}
            {candidateList}

            Classify the intent. Respond with JSON only: {jsonExample}
            """;

        var request = new ChatCompletionRequest(
            _options.Model,
            [
                new ChatMessage("system", systemPrompt),
                new ChatMessage("user", userPrompt)
            ],
            new ResponseFormat("json_object"));

        var response = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            ct => _httpClient.PostAsJsonAsync(_options.Endpoint ?? "chat/completions", request, ct),
            (_, body) => throw new HttpRequestException($"LLM rate limited: {body}"),
            cancellationToken: cancellationToken);

        var completion = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken);

        var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
            return new IntentClassificationResult("Unknown", 0, "Empty LLM response");

        try
        {
            var parsed = JsonSerializer.Deserialize<LlmClassificationOutput>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new IntentClassificationResult(
                parsed?.Intent ?? "Unknown",
                Math.Clamp(parsed?.Confidence ?? 0, 0, 1),
                parsed?.Reasoning);
        }
        catch (JsonException)
        {
            return new IntentClassificationResult("Unknown", 0, $"Failed to parse LLM response: {content}");
        }
    }

    private const string DefaultSystemPrompt =
        "You are an intent classification system. Given a sequence of behavior events (actor:action pairs), " +
        "determine the most likely user intent. Return ONLY valid JSON with fields: intent (string), " +
        "confidence (number 0-1), reasoning (string).";

    private sealed record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("response_format")] ResponseFormat? ResponseFormat = null);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ResponseFormat(
        [property: JsonPropertyName("type")] string Type);

    private sealed record ChatCompletionResponse(
        [property: JsonPropertyName("choices")] List<ChatChoice>? Choices);

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] ChatChoiceMessage? Message);

    private sealed record ChatChoiceMessage(
        [property: JsonPropertyName("content")] string? Content);

    private sealed record LlmClassificationOutput(
        [property: JsonPropertyName("intent")] string? Intent,
        [property: JsonPropertyName("confidence")] double? Confidence,
        [property: JsonPropertyName("reasoning")] string? Reasoning);
}
#pragma warning restore IDE0052

/// <summary>
/// Options for the prompt-based intent classifier.
/// </summary>
public sealed class PromptClassifierOptions
{
    public required string Model { get; init; }
    public string? Endpoint { get; init; }
    public string? SystemPrompt { get; init; }
}
