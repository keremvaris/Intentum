using Intentum.Explainability;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Tests;

public sealed class ExplainabilityExtensionsTests
{
    [Fact]
    public void AddIntentTreeExplainer_RegistersExplainer()
    {
        var services = new ServiceCollection();
        services.AddIntentTreeExplainer();
        var provider = services.BuildServiceProvider();

        var explainer = provider.GetService<IIntentTreeExplainer>();
        Assert.NotNull(explainer);
    }
}
