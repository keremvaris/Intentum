using Intentum.Persistence.EntityFramework;
using Intentum.Sample.Blazor;
using Intentum.Sample.Blazor.Features.GreenwashingDetection;
using Timer = System.Timers.Timer;

var builder = WebApplication.CreateBuilder(args);
ProgramConfiguration.ConfigureServices(builder);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IntentumDbContext>();
    await db.Database.EnsureCreatedAsync();
}

ProgramConfiguration.ConfigureMiddleware(app);
ProgramConfiguration.MapEndpoints(app);

var mockTimer = new Timer(30_000) { AutoReset = true };
mockTimer.Elapsed += (_, _) => GreenwashingRecentStore.AddMockEntry();
app.Lifetime.ApplicationStarted.Register(() => mockTimer.Start());
app.Lifetime.ApplicationStopping.Register(() => mockTimer.Stop());

await app.RunAsync();
