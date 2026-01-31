using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Simulation;

/// <summary>
/// Extension methods for registering simulation services.
/// </summary>
public static class SimulationExtensions
{
    /// <summary>
    /// Adds behavior space simulator and scenario runner to the service collection.
    /// </summary>
    public static IServiceCollection AddIntentScenarioRunner(this IServiceCollection services)
    {
        services.AddSingleton<IBehaviorSpaceSimulator, BehaviorSpaceSimulator>();
        services.AddSingleton<IScenarioRunner, IntentScenarioRunner>();
        return services;
    }
}
