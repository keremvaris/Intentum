using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Intentum.Events;

/// <summary>
/// Dispatches intent events to configured webhook URLs via HTTP POST with retry.
/// </summary>
public sealed class WebhookIntentEventHandler : IIntentEventHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IntentumEventsOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public WebhookIntentEventHandler(IHttpClientFactory httpClientFactory, IOptions<IntentumEventsOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        IntentEventPayload payload,
        IntentumEventType eventType,
        CancellationToken cancellationToken = default)
    {
        var eventTypeName = eventType.ToString();
        var webhooks = _options.Webhooks
            .Where(w => w.EventTypes.Contains(eventTypeName, StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var webhook in webhooks)
        {
            await SendWithRetryAsync(webhook.Url, payload, eventTypeName, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SendWithRetryAsync(
        string url,
        IntentEventPayload payload,
        string eventType,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(IntentumEventsOptions.HttpClientName);
        var dto = new WebhookPayloadDto(
            payload.BehaviorSpaceId,
            payload.Intent.Name,
            payload.Intent.Confidence.Level,
            payload.Intent.Confidence.Score,
            payload.Decision.ToString(),
            payload.RecordedAt,
            eventType);

        var maxAttempts = Math.Max(1, _options.RetryCount + 1);
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var response = await client.PostAsJsonAsync(url, dto, JsonOptions, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return;
            }
            catch (HttpRequestException) when (attempt < maxAttempts - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private sealed record WebhookPayloadDto(
        string? BehaviorSpaceId,
        string IntentName,
        string ConfidenceLevel,
        double ConfidenceScore,
        string Decision,
        DateTimeOffset RecordedAt,
        string EventType);
}
