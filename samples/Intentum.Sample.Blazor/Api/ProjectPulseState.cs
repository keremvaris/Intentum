namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// State for Project Pulse (The Pulse) scenario: running, variant, current step.
/// </summary>
public sealed class ProjectPulseState
{
    private volatile bool _running;
    private volatile string _currentVariant = ProjectPulseVariants.VariantA;
    private volatile int _currentStep;

    public bool Running => _running;
    public string CurrentVariant => _currentVariant;
    public int CurrentStep => _currentStep;

    public void Start(string? variant = null)
    {
        _running = true;
        _currentVariant = variant ?? ProjectPulseVariants.VariantA;
        _currentStep = 0;
    }

    public void Stop()
    {
        _running = false;
    }

    public void SetStep(int step) => _currentStep = step;
}
