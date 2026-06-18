using Intentum.Core.Behavior;
using Intentum.Tests.Helpers;

namespace Intentum.Tests.Persistence;

public sealed class InMemoryBehaviorSpaceRepositoryTests
{
    private readonly InMemoryBehaviorSpaceRepository _repo = new();

    [Fact]
    public async Task SaveAsync_ReturnsGeneratedId()
    {
        var space = new BehaviorSpace();

        var id = await _repo.SaveAsync(space);

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public async Task SaveAsync_StoresSpace()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));

        var id = await _repo.SaveAsync(space);

        var retrieved = await _repo.GetByIdAsync(id);
        Assert.NotNull(retrieved);
        Assert.Single(retrieved!.Events);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repo.GetByIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectSpace()
    {
        var space1 = new BehaviorSpace();
        space1.Observe(new BehaviorEvent("user1", "action1", DateTimeOffset.UtcNow));
        var space2 = new BehaviorSpace();
        space2.Observe(new BehaviorEvent("user2", "action2", DateTimeOffset.UtcNow));

        var id1 = await _repo.SaveAsync(space1);
        var id2 = await _repo.SaveAsync(space2);

        var retrieved1 = await _repo.GetByIdAsync(id1);
        var retrieved2 = await _repo.GetByIdAsync(id2);

        Assert.Equal("user1", retrieved1!.Events.First().Actor);
        Assert.Equal("user2", retrieved2!.Events.First().Actor);
    }

    [Fact]
    public async Task GetByMetadataAsync_ReturnsMatchingSpaces()
    {
        var space1 = new BehaviorSpace();
        space1.SetMetadata("sector", "finance");
        var space2 = new BehaviorSpace();
        space2.SetMetadata("sector", "healthcare");
        var space3 = new BehaviorSpace();
        space3.SetMetadata("sector", "finance");

        await _repo.SaveAsync(space1);
        await _repo.SaveAsync(space2);
        await _repo.SaveAsync(space3);

        var results = await _repo.GetByMetadataAsync("sector", "finance");

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetByMetadataAsync_ReturnsEmpty_WhenNoMatch()
    {
        var space = new BehaviorSpace();
        space.SetMetadata("sector", "finance");
        await _repo.SaveAsync(space);

        var results = await _repo.GetByMetadataAsync("sector", "retail");

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByTimeWindowAsync_FiltersByEventTime()
    {
        var oldSpace = new BehaviorSpace();
        oldSpace.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow.AddHours(-5)));

        var newSpace = new BehaviorSpace();
        newSpace.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));

        await _repo.SaveAsync(oldSpace);
        await _repo.SaveAsync(newSpace);

        var start = DateTimeOffset.UtcNow.AddMinutes(-1);
        var end = DateTimeOffset.UtcNow.AddMinutes(1);
        var results = await _repo.GetByTimeWindowAsync(start, end);

        Assert.Single(results);
    }

    [Fact]
    public async Task GetByTimeWindowAsync_ReturnsEmpty_WhenNoEventsInWindow()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow.AddHours(-10)));
        await _repo.SaveAsync(space);

        var start = DateTimeOffset.UtcNow.AddMinutes(-1);
        var end = DateTimeOffset.UtcNow.AddMinutes(1);
        var results = await _repo.GetByTimeWindowAsync(start, end);

        Assert.Empty(results);
    }

    [Fact]
    public async Task DeleteAsync_RemovesSpace()
    {
        var space = new BehaviorSpace();
        var id = await _repo.SaveAsync(space);

        var deleted = await _repo.DeleteAsync(id);

        Assert.True(deleted);
        var retrieved = await _repo.GetByIdAsync(id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var deleted = await _repo.DeleteAsync("nonexistent");

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAffectOtherSpaces()
    {
        var space1 = new BehaviorSpace();
        space1.Observe(new BehaviorEvent("user1", "action", DateTimeOffset.UtcNow));
        var space2 = new BehaviorSpace();
        space2.Observe(new BehaviorEvent("user2", "action", DateTimeOffset.UtcNow));

        var id1 = await _repo.SaveAsync(space1);
        var id2 = await _repo.SaveAsync(space2);

        await _repo.DeleteAsync(id1);

        var retrieved2 = await _repo.GetByIdAsync(id2);
        Assert.NotNull(retrieved2);
    }

    [Fact]
    public async Task SaveAsync_PreservesMetadata()
    {
        var space = new BehaviorSpace();
        space.SetMetadata("session", "abc123");
        space.SetMetadata("count", 42.0);

        var id = await _repo.SaveAsync(space);
        var retrieved = await _repo.GetByIdAsync(id);

        Assert.NotNull(retrieved);
        Assert.Equal("abc123", retrieved!.Metadata["session"]);
        Assert.Equal(42.0, retrieved.Metadata["count"]);
    }

    [Fact]
    public async Task SaveAsync_PreservesMultipleEvents()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow.AddMinutes(-2)));
        space.Observe(new BehaviorEvent("user", "click", DateTimeOffset.UtcNow.AddMinutes(-1)));
        space.Observe(new BehaviorEvent("user", "logout", DateTimeOffset.UtcNow));

        var id = await _repo.SaveAsync(space);
        var retrieved = await _repo.GetByIdAsync(id);

        Assert.NotNull(retrieved);
        Assert.Equal(3, retrieved!.Events.Count);
    }

    [Fact]
    public async Task MultipleSaves_AllStored()
    {
        for (int i = 0; i < 5; i++)
        {
            var space = new BehaviorSpace();
            space.Observe(new BehaviorEvent($"user{i}", "action", DateTimeOffset.UtcNow));
            await _repo.SaveAsync(space);
        }

        var start = DateTimeOffset.UtcNow.AddMinutes(-1);
        var end = DateTimeOffset.UtcNow.AddMinutes(1);
        var results = await _repo.GetByTimeWindowAsync(start, end);

        Assert.Equal(5, results.Count);
    }
}
