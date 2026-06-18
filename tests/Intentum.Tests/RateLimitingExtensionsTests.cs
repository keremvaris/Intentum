using Intentum.Runtime.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Tests;

public class RateLimitingExtensionsTests
{
    [Fact]
    public void AddIntentumRateLimiting_RegistersMemoryRateLimiter()
    {
        var services = new ServiceCollection();

        services.AddIntentumRateLimiting();

        var provider = services.BuildServiceProvider();
        var limiter = provider.GetService<IRateLimiter>();
        Assert.NotNull(limiter);
        Assert.IsType<MemoryRateLimiter>(limiter);
    }

    [Fact]
    public void AddIntentumRateLimiting_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddIntentumRateLimiting();

        Assert.Same(services, result);
    }
}
