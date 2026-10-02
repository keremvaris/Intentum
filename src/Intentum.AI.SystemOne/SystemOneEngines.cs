namespace Intentum.AI.SystemOne;

/// <summary>Presets for known System One compatible engines (Jev wire protocol).</summary>
public static class SystemOneEngines
{
    /// <summary>Known engine names accepted by <see cref="FromName"/>.</summary>
    public static IReadOnlyList<string> Names { get; } =
        ["demo", "jev", "kev", "tinyjev", "laya", "opendecision", "decision10", "openjev", "nanojev", "openjevpro", "foq"];

    /// <summary>
    /// In-process demo engine hosted by the consuming app itself (POST /v1/systemone).
    /// No API key, no model download; deterministic keyword-based decisions.
    /// <paramref name="baseUrl"/> is resolved at request time (app origin) by the host.
    /// The host must assign a <c>BaseUrl</c> before constructing a client or model; <see cref="SystemOneOptions.Validate"/> throws otherwise.
    /// </summary>
    public static SystemOneOptions Demo(string baseUrl = "") =>
        new() { BaseUrl = baseUrl, Engine = "demo", Model = "demo-latest" };

    /// <summary>
    /// Jev (TypeSafe AI): hosted System One model at <c>https://api.typesafe.ai</c>.
    /// Requires an API key from <c>console.typesafe.ai/keys</c>; picked up from
    /// <c>TYPESAFE_API_KEY</c> unless overridden via <c>ApiKey</c>.
    /// </summary>
    public static SystemOneOptions Jev(string baseUrl = "https://api.typesafe.ai") =>
        new()
        {
            BaseUrl = baseUrl,
            Engine = "jev",
            Model = "jev-latest",
            ApiKey = Environment.GetEnvironmentVariable("TYPESAFE_API_KEY"),
        };

    /// <summary>Kev (jaredpalmer/kev): local server, default port 8009.</summary>
    public static SystemOneOptions Kev(string baseUrl = "http://127.0.0.1:8009") =>
        new() { BaseUrl = baseUrl, Engine = "kev", Model = "kev-latest" };

    /// <summary>TinyJev (ankit-aglawe/tinyjev): <c>tinyjev serve</c>, default port 8077.</summary>
    public static SystemOneOptions TinyJev(string baseUrl = "http://127.0.0.1:8077") =>
        new() { BaseUrl = baseUrl, Engine = "tinyjev" };

    /// <summary>Laya (NandhaKishorM/laya): <c>laya-serve</c>, default port 8000.</summary>
    public static SystemOneOptions Laya(string baseUrl = "http://127.0.0.1:8000") =>
        new() { BaseUrl = baseUrl, Engine = "laya" };

    /// <summary>OpenDecision: local System One compatible server.</summary>
    public static SystemOneOptions OpenDecision(string baseUrl = "http://127.0.0.1:8000") =>
        new() { BaseUrl = baseUrl, Engine = "opendecision" };

    /// <summary>Decision 1.0: System One compatible server.</summary>
    public static SystemOneOptions Decision10(string baseUrl = "http://127.0.0.1:8000") =>
        new() { BaseUrl = baseUrl, Engine = "decision10" };

    /// <summary>Open-Jev: HTTP server speaking the TypeSafe System One API.</summary>
    public static SystemOneOptions OpenJev(string baseUrl = "http://127.0.0.1:8000") =>
        new() { BaseUrl = baseUrl, Engine = "openjev" };

    /// <summary>NanoJev: local System One compatible server.</summary>
    public static SystemOneOptions NanoJev(string baseUrl = "http://127.0.0.1:8000") =>
        new() { BaseUrl = baseUrl, Engine = "nanojev" };

    /// <summary>OpenJevPro / openjev-sglang: System One API over SGLang.</summary>
    public static SystemOneOptions OpenJevPro(string baseUrl = "http://127.0.0.1:8000") =>
        new() { BaseUrl = baseUrl, Engine = "openjevpro" };

    /// <summary>Foq: System One compatible server.</summary>
    public static SystemOneOptions Foq(string baseUrl = "http://127.0.0.1:8000") =>
        new() { BaseUrl = baseUrl, Engine = "foq" };

    /// <summary>Resolves a preset by engine name (case-insensitive).</summary>
    /// <param name="name">Engine name; see <see cref="Names"/>.</param>
    /// <param name="baseUrl">Optional base URL override.</param>
    /// <exception cref="ArgumentException">Thrown when the engine name is unknown.</exception>
    public static SystemOneOptions FromName(string name, string? baseUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var options = name.Trim().ToLowerInvariant() switch
        {
            "demo" => Demo(),
            "jev" => Jev(),
            "kev" => Kev(),
            "tinyjev" => TinyJev(),
            "laya" => Laya(),
            "opendecision" => OpenDecision(),
            "decision10" => Decision10(),
            "openjev" => OpenJev(),
            "nanojev" => NanoJev(),
            "openjevpro" => OpenJevPro(),
            "foq" => Foq(),
            _ => throw new ArgumentException(
                $"Unknown System One engine '{name}'. Known engines: {string.Join(", ", Names)}.",
                nameof(name)),
        };

        return baseUrl is null ? options : options with { BaseUrl = baseUrl };
    }
}
