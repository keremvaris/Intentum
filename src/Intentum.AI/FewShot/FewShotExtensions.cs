using Intentum.AI.FewShot;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI;

public static class FewShotExtensions
{
    public static IServiceCollection AddIntentumFewShotStore(
        this IServiceCollection services)
    {
        services.AddSingleton<IFewShotStore, MemoryFewShotStore>();
        return services;
    }

    public static IServiceCollection AddIntentumFewShotModel(
        this IServiceCollection services)
    {
        services.AddSingleton<IIntentModel, FewShotIntentModel>();
        return services;
    }
}
