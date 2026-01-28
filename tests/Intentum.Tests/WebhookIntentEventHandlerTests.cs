using System.Net;
using System.Text.Json;
using Intentum.Core.Intent;
using Intentum.Core.Intents;
using Intentum.Events;
using Intentum.Runtime.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Intent = Intentum.Core.Intents.Intent;

namespace Intentum.Tests;

public class WebhookIntentEventHandlerTests
{
    private static IntentEventPayload CreatePayload(string? behaviorSpaceId = "bs-1")
    {
        var intent = new Intent(
            "Checkout",
            new List<IntentSignal> { new("cart", "view cart", 1.0) },
            new IntentConfidence(0.85, "High"));
        return new IntentEventPayload(behaviorSpaceId, intent, PolicyDecision.Allow, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void AddWebhook_DefaultEventTypes_IncludesIntentInferredAndPolicyDecisionChanged()
    {
        var options = new IntentumEventsOptions();
        options.AddWebhook("https://example.com/hook");
        Assert.Single(options.Webhooks);
        Assert.Contains("IntentInferred", options.Webhooks[0].EventTypes, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("PolicyDecisionChanged", options.Webhooks[0].EventTypes, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddWebhook_WithSpecificEvents_OnlyThoseEventTypes()
    {
        var options = new IntentumEventsOptions();
        options.AddWebhook("https://example.com/hook", new[] { "IntentInferred" });
        Assert.Single(options.Webhooks);
        Assert.Single(options.Webhooks[0].EventTypes);
        Assert.Contains("IntentInferred", options.Webhooks[0].EventTypes, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_NoWebhooksMatchingEventType_DoesNotThrow()
    {
        var options = new IntentumEventsOptions();
        options.AddWebhook("https://example.com/hook", new[] { "IntentInferred" });
        var handler = new CaptureHttpHandler();
        var handlerFactory = CreateHttpClientFactory(handler);
        var eventHandler = new WebhookIntentEventHandler(handlerFactory, Options.Create(options));
        var payload = CreatePayload();

        await eventHandler.HandleAsync(payload, IntentumEventType.PolicyDecisionChanged);

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task HandleAsync_WebhookConfigured_PostsToUrlWithCorrectBody()
    {
        var options = new IntentumEventsOptions();
        options.AddWebhook("https://example.com/hook", new[] { "IntentInferred" });
        var handler = new CaptureHttpHandler();
        var handlerFactory = CreateHttpClientFactory(handler);
        var eventHandler = new WebhookIntentEventHandler(handlerFactory, Options.Create(options));
        var payload = CreatePayload("space-42");
        var before = DateTimeOffset.UtcNow;

        await eventHandler.HandleAsync(payload, IntentumEventType.IntentInferred);

        Assert.Single(handler.Requests);
        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Equal("https://example.com/hook", req.RequestUri!.ToString());
        var body = await req.Content!.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.Equal("space-42", root.GetProperty("behaviorSpaceId").GetString());
        Assert.Equal("Checkout", root.GetProperty("intentName").GetString());
        Assert.Equal("High", root.GetProperty("confidenceLevel").GetString());
        Assert.Equal(0.85, root.GetProperty("confidenceScore").GetDouble());
        Assert.Equal("Allow", root.GetProperty("decision").GetString());
        Assert.Equal("IntentInferred", root.GetProperty("eventType").GetString());
    }

    [Fact]
    public async Task HandleAsync_MultipleWebhooks_SendsToAllMatching()
    {
        var options = new IntentumEventsOptions();
        options.AddWebhook("https://a.com/hook", new[] { "IntentInferred" });
        options.AddWebhook("https://b.com/hook", new[] { "IntentInferred" });
        var handler = new CaptureHttpHandler();
        var handlerFactory = CreateHttpClientFactory(handler);
        var eventHandler = new WebhookIntentEventHandler(handlerFactory, Options.Create(options));
        var payload = CreatePayload();

        await eventHandler.HandleAsync(payload, IntentumEventType.IntentInferred);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("https://a.com/hook", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal("https://b.com/hook", handler.Requests[1].RequestUri!.ToString());
    }

    [Fact]
    public async Task HandleAsync_HttpSuccess_CompletesWithoutRetry()
    {
        var options = new IntentumEventsOptions();
        options.AddWebhook("https://example.com/hook", new[] { "IntentInferred" });
        var handler = new CaptureHttpHandler { StatusCode = HttpStatusCode.OK };
        var handlerFactory = CreateHttpClientFactory(handler);
        var eventHandler = new WebhookIntentEventHandler(handlerFactory, Options.Create(options));
        var payload = CreatePayload();

        await eventHandler.HandleAsync(payload, IntentumEventType.IntentInferred);

        Assert.Single(handler.Requests);
    }

    [Fact]
    public void AddIntentumEvents_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddIntentumEvents(opt => opt.AddWebhook("https://example.com/hook"));
        var sp = services.BuildServiceProvider();
        var handler = sp.GetService<IIntentEventHandler>();
        Assert.NotNull(handler);
        Assert.IsType<WebhookIntentEventHandler>(handler);
    }

    private static IHttpClientFactory CreateHttpClientFactory(CaptureHttpHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };
        var factory = new MockHttpClientFactory(client);
        return factory;
    }

    private sealed class CaptureHttpHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(StatusCode));
        }
    }

    private sealed class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public MockHttpClientFactory(HttpClient client) => _client = client;

        public HttpClient CreateClient(string name) => _client;
    }
}
