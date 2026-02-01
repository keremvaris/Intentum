namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// In-memory store for dashboard config (GET/PUT from API).
/// </summary>
public sealed class DashboardConfigStore
{
    private volatile DashboardConfig _config = new();

    public DashboardConfig Get() => _config;

    public void Set(DashboardConfig config) => _config = config;
}
