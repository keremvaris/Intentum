using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Intentum.Sample.Blazor;
using Intentum.Sample.Blazor.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Intentum.Sample.Blazor.Tests.SystemOne;

public sealed class SystemOneHealthTests : IAsyncLifetime
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
    public async Task Health_DemoIsUp_AndUnverifiedEngineIsUnknown()
    {
        var response = await _client.GetAsync("/api/intent/systemone/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.EnumerateArray()
            .ToDictionary(
                i => i.GetProperty("engine").GetString()!,
                i => (Status: i.GetProperty("status").GetString()!, Hint: i.GetProperty("hint").GetString()!));

        Assert.Equal("up", items["demo"].Status);
        Assert.False(string.IsNullOrWhiteSpace(items["demo"].Hint));
        Assert.Equal("unknown", items["nanojev"].Status);
        Assert.True(items.ContainsKey("kev"));
        Assert.True(items.ContainsKey("jev"));
    }

    [Fact]
    public async Task Probe_ClosedLoopbackPort_ReturnsDown()
    {
        var freePort = GetFreePort();

        var item = await SystemOneHealth.ProbeAsync("custom", $"http://127.0.0.1:{freePort}");

        Assert.Equal("down", item.Status);
        Assert.Equal("custom", item.Engine);
    }

    [Fact]
    public async Task Probe_DemoEngine_IsUp_WithoutNetwork()
    {
        var item = await SystemOneHealth.ProbeAsync("demo", null);

        Assert.Equal("up", item.Status);
    }

    [Fact]
    public async Task Probe_RunningLoopbackServer_IsUp()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = new Task(() =>
        {
            while (true)
            {
                var socket = listener.AcceptTcpClient();
                var stream = socket.GetStream();
                var buffer = new byte[1024];
                _ = stream.Read(buffer, 0, buffer.Length);
                var response = "HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok";
                var bytes = System.Text.Encoding.ASCII.GetBytes(response);
                stream.Write(bytes, 0, bytes.Length);
                socket.Close();
            }
        });
        server.Start();

        try
        {
            var item = await SystemOneHealth.ProbeAsync("custom", $"http://127.0.0.1:{port}");

            Assert.Equal("up", item.Status);
        }
        finally
        {
            listener.Stop();
        }
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
