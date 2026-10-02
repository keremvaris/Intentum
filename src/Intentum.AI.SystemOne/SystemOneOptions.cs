namespace Intentum.AI.SystemOne;

/// <summary>Options for a System One compatible decision engine (POST /v1/systemone).</summary>
public sealed record SystemOneOptions
{
    /// <summary>Base URL of the engine, e.g. <c>http://127.0.0.1:8009</c>.</summary>
    public string BaseUrl { get; init; } = "";

    /// <summary>Engine name (kev, laya, tinyjev, ...). Used as the source of intent signals.</summary>
    public string Engine { get; init; } = "systemone";

    /// <summary>Optional API key sent as <c>Authorization: Bearer</c>.</summary>
    public string? ApiKey { get; init; }

    /// <summary>Endpoint path. Default is the System One <c>/v1/systemone</c>.</summary>
    public string EndpointPath { get; init; } = "/v1/systemone";

    /// <summary>Optional model/checkpoint name sent in the request (e.g. <c>kev-latest</c>).</summary>
    public string? Model { get; init; }

    /// <summary>Intent catalog: intent name → description. Becomes the options of the intent choice question.</summary>
    public IReadOnlyDictionary<string, string> IntentCatalog { get; init; } = new Dictionary<string, string>();

    /// <summary>Instructions for the intent choice question.</summary>
    public string IntentQuestionInstructions { get; init; } = "Which intent best explains the observed behavior?";

    /// <summary>Validates required options.</summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="BaseUrl"/> is empty.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException("SystemOneOptions.BaseUrl is required (e.g. http://127.0.0.1:8000).");
    }
}
