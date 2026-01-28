// Test file to check if FusionCache is accessible
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.Caching.FusionCache;

public static class TestFusionCache
{
    public static void Test()
    {
        var services = new ServiceCollection();
        
        // Try to use FusionCache extension methods
        // This will fail at compile time if namespace is wrong
        // services.AddFusionCache();
    }
}
