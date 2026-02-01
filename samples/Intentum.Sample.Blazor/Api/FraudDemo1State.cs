namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// State for Demo 1 (Finans Dolandırıcılığı) scenario: running flag and step index.
/// </summary>
public sealed class FraudDemo1State
{
    private volatile bool _running;
    private volatile int _currentStep;

    public bool Running => _running;
    public int CurrentStep => _currentStep;

    public void Start()
    {
        _running = true;
        _currentStep = 0;
    }

    public void Stop()
    {
        _running = false;
    }

    public void SetStep(int step) => _currentStep = step;
}
