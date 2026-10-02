using System.Net;
using System.Text;
using System.Text.Json;
using Intentum.AI.SystemOne;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// In-process transport for the demo engine: takes the serialized POST /v1/systemone
/// payload produced by <see cref="SystemOneClient"/> and answers it via
/// <see cref="SystemOneDemoEngine"/> without opening a socket. The adapter's JSON
/// serialization path stays fully exercised; only the HTTP hop is virtual.
/// </summary>
internal sealed class DemoEngineHttpHandler : HttpMessageHandler
{
    internal static readonly DemoEngineHttpHandler Instance = new();

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var content = request.Content
            ?? throw new InvalidOperationException("System One request body is required.");

        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = doc.RootElement;

        var state = root.TryGetProperty("state", out var stateElement)
            ? stateElement.Clone()
            : default;

        Dictionary<string, SystemOneQuestion>? questions = null;
        if (root.TryGetProperty("questions", out var questionsElement)
            && questionsElement.ValueKind == JsonValueKind.Object)
        {
            questions = questionsElement.Deserialize<Dictionary<string, SystemOneQuestion>>(JsonOptions);
        }

        var response = SystemOneDemoEngine.Decide(state, questions);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(response),
                Encoding.UTF8,
                "application/json"),
        };
    }
}
