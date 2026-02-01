using Intentum.Simulation;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Tests;

public sealed class SimulationExtensionsTests
{
    [Fact]
    public void AddIntentScenarioRunner_RegistersSimulatorAndRunner()
    {
        var services = new ServiceCollection();
        services.AddIntentScenarioRunner();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IBehaviorSpaceSimulator>());
        Assert.NotNull(provider.GetService<IScenarioRunner>());
    }
}
