using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Tests for BehaviorSpace sanitization (anonymization/masking for GDPR/CCPA).
/// </summary>
public sealed class BehaviorSpaceSanitizationTests
{
    [Fact]
    public void Sanitize_MasksActorAndAction_WhenOptionsDefault()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user-123", "login", DateTimeOffset.UtcNow));
        space.Observe(new BehaviorEvent("user-456", "submit", DateTimeOffset.UtcNow));

        var sanitized = space.Sanitize(new SanitizationOptions(MaskActor: true, MaskAction: true, UseHash: true));

        Assert.Equal(2, sanitized.Events.Count);
        Assert.All(sanitized.Events, evt =>
        {
            _ = evt;
            Assert.NotEqual("user-123", evt.Actor);
            Assert.NotEqual("user-456", evt.Actor);
            Assert.NotEqual("login", evt.Action);
            Assert.NotEqual("submit", evt.Action);
            Assert.True(evt.Actor.Length > 0);
            Assert.True(evt.Action.Length > 0);
        });
    }

    [Fact]
    public void Sanitize_WithPlaceholder_ReplacesWithPlaceholder()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("alice", "click", DateTimeOffset.UtcNow));

        var sanitized = space.Sanitize(new SanitizationOptions(UseHash: false, Placeholder: "***"));

        Assert.Single(sanitized.Events);
        Assert.Equal("***", sanitized.Events.First().Actor);
        Assert.Equal("***", sanitized.Events.First().Action);
    }

    [Fact]
    public void Sanitize_RedactsMetadataKeys()
    {
        var space = new BehaviorSpace();
        space.SetMetadata("email", "a@b.com");
        space.SetMetadata("sector", "retail");
        space.Observe(new BehaviorEvent("u", "a", DateTimeOffset.UtcNow, new Dictionary<string, object> { ["email"] = "x@y.com" }));

        var sanitized = space.Sanitize(new SanitizationOptions(MaskActor: false, MaskAction: false, MetadataKeysToRedact: ["email"]));

        Assert.Equal("[redacted]", sanitized.Metadata["email"]);
        Assert.Equal("retail", sanitized.Metadata["sector"]);
        Assert.Equal("[redacted]", sanitized.Events.First().Metadata!["email"]);
    }

    [Fact]
    public void Sanitize_DoesNotModifyOriginal()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));

        _ = space.Sanitize();

        Assert.Equal("user", space.Events.First().Actor);
        Assert.Equal("login", space.Events.First().Action);
    }
}
