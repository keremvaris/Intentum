using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Tests for BehaviorSpace metadata and time-windowed analysis.
/// </summary>
public class BehaviorSpaceMetadataTests
{
    [Fact]
    public void BehaviorSpace_SetMetadata_StoresMetadata()
    {
        // Arrange
        var space = new BehaviorSpace();

        // Act
        space.SetMetadata("sector", "ESG");
        space.SetMetadata("sessionId", "abc123");

        // Assert
        Assert.Equal("ESG", space.GetMetadata<string>("sector"));
        Assert.Equal("abc123", space.GetMetadata<string>("sessionId"));
    }

    [Fact]
    public void BehaviorSpace_GetMetadata_ReturnsDefaultWhenNotFound()
    {
        // Arrange
        var space = new BehaviorSpace();

        // Act
        var result = space.GetMetadata<string>("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void BehaviorSpace_GetEventsInWindow_FiltersByTime()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", now.AddHours(-2)));
        space.Observe(new BehaviorEvent("user", "submit", now.AddMinutes(-30)));
        space.Observe(new BehaviorEvent("user", "retry", now));

        // Act
        var eventsInLastHour = space.GetEventsInWindow(TimeSpan.FromHours(1));

        // Assert
        Assert.Equal(2, eventsInLastHour.Count);
        Assert.Contains(eventsInLastHour, e => e.Action == "submit");
        Assert.Contains(eventsInLastHour, e => e.Action == "retry");
    }

    [Fact]
    public void BehaviorSpace_GetEventsInWindow_WithDateTimeRange_FiltersCorrectly()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow.AddHours(-2);
        var end = DateTimeOffset.UtcNow.AddHours(-1);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", start.AddMinutes(-30)));
        space.Observe(new BehaviorEvent("user", "submit", start.AddMinutes(30)));
        space.Observe(new BehaviorEvent("user", "retry", end.AddMinutes(30)));

        // Act
        var eventsInWindow = space.GetEventsInWindow(start, end);

        // Assert
        Assert.Single(eventsInWindow);
        Assert.Equal("submit", eventsInWindow.First().Action);
    }

    [Fact]
    public void BehaviorSpace_GetTimeSpan_ReturnsCorrectSpan()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", now.AddHours(-2)));
        space.Observe(new BehaviorEvent("user", "submit", now));

        // Act
        var span = space.GetTimeSpan();

        // Assert
        Assert.NotNull(span);
        Assert.True(span.Value.TotalHours >= 1.9 && span.Value.TotalHours <= 2.1);
    }

    [Fact]
    public void BehaviorSpace_GetTimeSpan_EmptySpace_ReturnsNull()
    {
        // Arrange
        var space = new BehaviorSpace();

        // Act
        var span = space.GetTimeSpan();

        // Assert
        Assert.Null(span);
    }

    [Fact]
    public void BehaviorSpace_ToVector_WithTimeWindow_FiltersEvents()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", now.AddHours(-2)));
        space.Observe(new BehaviorEvent("user", "submit", now.AddMinutes(-30)));
        space.Observe(new BehaviorEvent("user", "retry", now));

        // Act
        var vector = space.ToVector(now.AddHours(-1), now);

        // Assert
        Assert.Equal(2, vector.Dimensions.Count);
        Assert.Contains("user:submit", vector.Dimensions.Keys);
        Assert.Contains("user:retry", vector.Dimensions.Keys);
    }
}
