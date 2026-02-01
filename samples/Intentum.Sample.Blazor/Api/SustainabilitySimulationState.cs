namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Shared state for sustainability timeline simulation (start/stop, company, granularity, simulated date). Thread-safe.
/// </summary>
public sealed class SustainabilitySimulationState
{
    private volatile bool _running;
    private string _companyId = "shell";
    private string _granularity = "Monthly";
    private DateTimeOffset _simulatedAt;
    private DateTimeOffset _startAt;
    private readonly object _lock = new();

    public bool Running => _running;
    public string CompanyId => _companyId;
    public string Granularity => _granularity;
    public DateTimeOffset SimulatedAt => _simulatedAt;
    public DateTimeOffset StartAt => _startAt;

    public void Start(string companyId, string granularity)
    {
        lock (_lock)
        {
            _companyId = string.IsNullOrWhiteSpace(companyId) ? "shell" : companyId;
            _granularity = string.IsNullOrWhiteSpace(granularity) ? "Monthly" : granularity;
            _startAt = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            _simulatedAt = _startAt;
            _running = true;
        }
    }

    public void Stop()
    {
        lock (_lock) { _running = false; }
    }

    /// <summary>
    /// Advances simulated date by one unit (hour/day/month) according to current granularity. Returns the new SimulatedAt.
    /// </summary>
    public DateTimeOffset Advance()
    {
        lock (_lock)
        {
            _simulatedAt = _granularity switch
            {
                "Hourly" => _simulatedAt.AddHours(1),
                "Daily" => _simulatedAt.AddDays(1),
                _ => _simulatedAt.AddMonths(1) // Monthly default
            };
            return _simulatedAt;
        }
    }
}

/// <summary>Request body for POST /api/sustainability-simulation/start.</summary>
public sealed record SustainabilityStartRequest(
    [property: System.Text.Json.Serialization.JsonPropertyName("companyId")] string? CompanyId,
    [property: System.Text.Json.Serialization.JsonPropertyName("granularity")] string? Granularity);
