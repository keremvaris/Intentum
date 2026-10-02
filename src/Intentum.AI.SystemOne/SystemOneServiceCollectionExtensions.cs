using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.AI.SystemOne;

/// <summary>DI registration for the System One adapter.</summary>
public static class SystemOneServiceCollectionExtensions
{
    /// <summary>Registers <see cref="SystemOneIntentModel"/> as the <see cref="IIntentModel"/>.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="options">Engine options (BaseUrl, IntentCatalog, ...).</param>
    public static IServiceCollection AddIntentumSystemOne(this IServiceCollection services, SystemOneOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        services.AddSingleton<IIntentModel>(_ => new SystemOneIntentModel(options, new HttpClient()));

        return services;
    }
}
