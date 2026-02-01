namespace Intentum.Sample.Web.Api;

/// <summary>
/// Shared state for fraud simulation (start/stop, stats). Thread-safe.
/// </summary>
public sealed class FraudSimulationState
{
    private volatile bool _running;
    private int _inferencesLastMinute;
    private DateTimeOffset _lastInferenceAt;
    private DateTimeOffset _minuteStart = DateTimeOffset.UtcNow;

    public bool Running => _running;
    public int EventsPerMinute => Interlocked.CompareExchange(ref _inferencesLastMinute, 0, 0);
    public DateTimeOffset LastInferenceAt => _lastInferenceAt;

    public void Start() => _running = true;
    public void Stop() => _running = false;

    public void RecordInference()
    {
        var now = DateTimeOffset.UtcNow;
        _lastInferenceAt = now;
        if (now - _minuteStart >= TimeSpan.FromMinutes(1))
        {
            _minuteStart = now;
            Interlocked.Exchange(ref _inferencesLastMinute, 0);
        }
        Interlocked.Increment(ref _inferencesLastMinute);
    }
}
