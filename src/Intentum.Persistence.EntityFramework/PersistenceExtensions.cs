using Intentum.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Persistence.EntityFramework;

/// <summary>
/// Extension methods for Entity Framework Core persistence.
/// </summary>
public static class PersistenceExtensions
{
    /// <summary>
    /// Adds Entity Framework Core persistence for Intentum.
    /// </summary>
    public static IServiceCollection AddIntentumPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        services.AddDbContext<IntentumDbContext>(configureOptions);
        services.AddScoped<IBehaviorSpaceRepository, BehaviorSpaceRepository>();
        services.AddScoped<IIntentHistoryRepository, IntentHistoryRepository>();
        return services;
    }

    /// <summary>
    /// Adds Entity Framework Core persistence with in-memory database (for testing).
    /// </summary>
    public static IServiceCollection AddIntentumPersistenceInMemory(
        this IServiceCollection services,
        string databaseName = "IntentumTest")
    {
        return services.AddIntentumPersistence(options =>
            options.UseInMemoryDatabase(databaseName));
    }

    /// <summary>
    /// Ensures the Intentum database schema exists. Uses EnsureCreatedAsync for in-memory
    /// and MigrateAsync for relational databases when migrations are available.
    /// Call during application startup.
    /// </summary>
    public static async Task InitializeIntentumDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IntentumDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
