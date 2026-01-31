using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Explainability;

/// <summary>
/// Extension methods for registering intent explainability services.
/// </summary>
public static class ExplainabilityExtensions
{
    /// <summary>
    /// Adds intent tree explainer (root-cause decision tree) to the service collection.
    /// </summary>
    public static IServiceCollection AddIntentTreeExplainer(this IServiceCollection services)
    {
        services.AddSingleton<IIntentTreeExplainer, IntentTreeExplainer>();
        return services;
    }
}
