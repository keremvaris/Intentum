using Intentum.Core.Behavior;

namespace Intentum.Tests.Core.Behavior;

public class PiiDetectionTests
{
    [Fact]
    public void Sanitize_EmailAddress_MasksCorrectly()
    {
        var options = new SanitizationOptions(
            MaskActor: false,
            MaskAction: false,
            MaskEmails: true);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow, new Dictionary<string, object>
        {
            ["email"] = "test@example.com"
        }));

        var sanitized = space.Sanitize(options);
        var email = sanitized.Events.First().Metadata!["email"].ToString()!;

        Assert.Matches(@"\w+@\*\*\*\.\*\*\*", email);
    }

    [Fact]
    public void Sanitize_PhoneNumber_MasksCorrectly()
    {
        var options = new SanitizationOptions(
            MaskActor: false,
            MaskAction: false,
            MaskPhoneNumbers: true);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "call", DateTimeOffset.UtcNow, new Dictionary<string, object>
        {
            ["phone"] = "+1-555-123-4567"
        }));

        var sanitized = space.Sanitize(options);
        var phone = sanitized.Events.First().Metadata!["phone"].ToString()!;

        Assert.DoesNotContain("555", phone);
        Assert.Contains("+", phone);
    }

    [Fact]
    public void Sanitize_WithoutPiiDetection_DoesNotMaskEmails()
    {
        var options = new SanitizationOptions(
            MaskActor: false,
            MaskAction: false,
            MaskEmails: false);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow, new Dictionary<string, object>
        {
            ["email"] = "test@example.com"
        }));

        var sanitized = space.Sanitize(options);

        Assert.Equal("test@example.com", sanitized.Events.First().Metadata!["email"]);
    }
}
