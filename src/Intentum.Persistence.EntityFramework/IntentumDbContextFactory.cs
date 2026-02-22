using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Intentum.Persistence.EntityFramework;

/// <summary>
/// Design-time factory for creating IntentumDbContext for EF Core migrations.
/// Configure INTENTUM_DB_CONNECTION environment variable to set the connection string.
/// Default uses in-memory database for development.
/// Usage: dotnet ef migrations add InitialCreate -p src/Intentum.Persistence.EntityFramework
/// </summary>
public sealed class IntentumDbContextFactory : IDesignTimeDbContextFactory<IntentumDbContext>
{
    public IntentumDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IntentumDbContext>();
        optionsBuilder.UseInMemoryDatabase("IntentumDesignTime");
        return new IntentumDbContext(optionsBuilder.Options);
    }
}
