using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Sample.Blazor;
using Intentum.AI.SystemOne;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Intentum.Sample.Blazor.Tests.SystemOne;

[Collection("SystemOneEnv")]
public sealed class SystemOneEndpointsTests : IAsyncLifetime
{
    private IHost _host = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        ProgramConfiguration.MapSystemOneEndpoints(app);
        await app.StartAsync();
        _host = app;
        _client = app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
        _client.Dispose();
    }

    [Fact]
    public void DemoEngine_RoundTripsThroughInProcessEndpoint()
    {
        var model = new SystemOneIntentModel(
            SystemOneEngines.Demo("http://systemone.test") with
            {
                IntentCatalog = ProgramConfiguration.SystemOneIntentCatalog,
            },
            _client);
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
            .Action("payment.charged_twice")
            .Build();

        var intent = model.Infer(space);

        Assert.Equal("Billing", intent.Name);
        Assert.True(intent.Confidence.Score > 0);
        Assert.Contains(intent.Signals, s => s.Source == "demo" && s.Description == "Billing");
        Assert.Equal(4, intent.Signals.Count);
        Assert.Contains(intent.Signals, s => s.Description == "Other");
        Assert.NotNull(intent.Reasoning);
    }

    [Fact]
    public async Task Infer_WithoutEngineQuery_DefaultsToDemo()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_ENGINE", null);
        Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", null);
        try
        {
            var response = await _client.PostAsJsonAsync(
                "/api/intent/systemone/infer",
                new { events = new[] { new { actor = "user", action = "refund.requested" } } });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("demo", json.GetProperty("engine").GetString());
            Assert.Equal("Billing", json.GetProperty("name").GetString());
        }
        finally
        {
            Environment.SetEnvironmentVariable("SYSTEMONE_ENGINE", null);
            Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", null);
        }
    }

    [Fact]
    public async Task Infer_UnknownEngine_Returns400WithHumanMessage()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/intent/systemone/infer?engine=does-not-exist",
            new { events = new[] { new { actor = "user", action = "login" } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("Unknown System One engine", json.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Infer_ApiKeyInBody_IsAccepted_AndNeverAppearsInQueryString()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/intent/systemone/infer?engine=demo",
            new
            {
                events = new[] { new { actor = "user", action = "login" } },
                apiKey = "visitor-secret",
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("visitor-secret", response.RequestMessage!.RequestUri!.Query);
    }
}
