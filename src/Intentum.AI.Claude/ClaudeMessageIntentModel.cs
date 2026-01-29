using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.Claude;

public sealed class ClaudeMessageIntentModel : IIntentModel
{
    private readonly ClaudeOptions _options;
    private readonly HttpClient _httpClient;

    public ClaudeMessageIntentModel(ClaudeOptions options, HttpClient httpClient)
    {
        _options = options;
        _httpClient = httpClient;
    }

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        _options.Validate();

        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var behaviors = string.Join(", ", vector.Dimensions.Keys);

        var prompt =
            "You are an intent scoring engine. " +
            "Return ONLY a number between 0 and 1. " +
            $"Behavior keys: {behaviors}";

        var request = new ClaudeMessageRequest(
            _options.Model,
            32,
            [new ClaudeMessage("user", prompt)]);

        var response = _httpClient
            .PostAsJsonAsync("messages", request)
            .GetAwaiter()
            .GetResult();

        response.EnsureSuccessStatusCode();

        var payload = response.Content
            .ReadFromJsonAsync<ClaudeMessageResponse>()
            .GetAwaiter()
            .GetResult();

        var text = payload?.Content?.FirstOrDefault()?.Text ?? "0.5";
        var score = ParseScore(text);
        var confidence = IntentConfidence.FromScore(score);

        var signals = vector.Dimensions.Keys.Select(k =>
            new IntentSignal(
                Source: "claude",
                Description: k,
                Weight: score)).ToList();

        return new Intent(
            Name: "AI-Inferred-Intent",
            Signals: signals,
            Confidence: confidence
        );
    }

    private static double ParseScore(string text)
    {
        if (double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return Math.Clamp(value, 0.0, 1.0);

        var first = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
        if (double.TryParse(first.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return Math.Clamp(value, 0.0, 1.0);

        return 0.5;
    }

    private sealed record ClaudeMessageRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("messages")] IReadOnlyList<ClaudeMessage> Messages);

    private sealed record ClaudeMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ClaudeMessageResponse(
        [property: JsonPropertyName("content")] List<ClaudeContent> Content);

    private sealed record ClaudeContent(
        [property: JsonPropertyName("text")] string Text);
}
