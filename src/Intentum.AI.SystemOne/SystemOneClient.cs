using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Intentum.AI.SystemOne;

/// <summary>HTTP client for a System One compatible <c>POST /v1/systemone</c> endpoint.</summary>
public sealed class SystemOneClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly SystemOneOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>Creates a client for the given engine options.</summary>
    public SystemOneClient(SystemOneOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>Sends the state and typed questions and returns the answers synchronously.</summary>
    /// <param name="state">State to evaluate (string or object).</param>
    /// <param name="questions">Typed questions keyed by your own id.</param>
    public SystemOneResponse Decide(object state, IReadOnlyDictionary<string, SystemOneQuestion> questions) =>
        DecideAsync(state, questions).GetAwaiter().GetResult();

    /// <summary>Sends the state and typed questions and returns the answers.</summary>
    /// <param name="state">State to evaluate (string or object).</param>
    /// <param name="questions">Typed questions keyed by your own id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<SystemOneResponse> DecideAsync(
        object state,
        IReadOnlyDictionary<string, SystemOneQuestion> questions,
        CancellationToken cancellationToken = default)
    {
        _options.Validate();
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(questions);

        var request = new SystemOneRequest(state, questions, _options.Model);
        var json = JsonSerializer.SerializeToUtf8Bytes(request, JsonOptions);
        using var message = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri())
        {
            Content = new ByteArrayContent(json)
            {
                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") { CharSet = "utf-8" } }
            }
        };

        if (_options.ApiKey is not null)
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var response = await _httpClient
            .SendAsync(message, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<SystemOneResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return payload ?? throw new InvalidOperationException("System One engine returned an empty response.");
    }

    private Uri BuildRequestUri()
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var path = _options.EndpointPath.StartsWith('/') ? _options.EndpointPath : "/" + _options.EndpointPath;
        return new Uri(baseUrl + path);
    }

    private sealed record SystemOneRequest(
        [property: JsonPropertyName("state")] object State,
        [property: JsonPropertyName("questions")] IReadOnlyDictionary<string, SystemOneQuestion> Questions,
        [property: JsonPropertyName("model")]
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? Model);
}
