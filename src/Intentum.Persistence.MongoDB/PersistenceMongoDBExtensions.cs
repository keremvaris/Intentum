using Intentum.Persistence.Repositories;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Intentum.Persistence.MongoDB;

/// <summary>
/// Extension methods for MongoDB persistence.
/// </summary>
[UsedImplicitly]
public static class PersistenceMongoDBExtensions
{
    /// <summary>
    /// Adds MongoDB persistence for Intentum (behavior spaces and intent history).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="database">The MongoDB database instance.</param>
    /// <param name="behaviorSpaceCollectionName">Optional collection name for behavior spaces (default: "behaviorspaces").</param>
    /// <param name="intentHistoryCollectionName">Optional collection name for intent history (default: "intenthistory").</param>
    [UsedImplicitly]
    public static IServiceCollection AddIntentumPersistenceMongoDB(
        this IServiceCollection services,
        IMongoDatabase database,
        string behaviorSpaceCollectionName = "behaviorspaces",
        string intentHistoryCollectionName = "intenthistory")
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (database == null)
            throw new ArgumentNullException(nameof(database));

        services.AddSingleton<IBehaviorSpaceRepository>(_ =>
            new MongoBehaviorSpaceRepository(database, behaviorSpaceCollectionName));
        services.AddSingleton<IIntentHistoryRepository>(_ =>
            new MongoIntentHistoryRepository(database, intentHistoryCollectionName));
        return services;
    }
}
