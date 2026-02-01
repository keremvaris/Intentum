using Intentum.Core.Behavior;
using Intentum.MultiTenancy;
using Intentum.Persistence.Repositories;

namespace Intentum.Tests;

public class TenantAwareBehaviorSpaceRepositoryTests
{
    [Fact]
    public async Task SaveAsync_InjectsTenantIdIntoMetadata()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider("tenant-1");
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));

        var id = await repo.SaveAsync(space);

        Assert.NotEmpty(id);
        var saved = await inner.GetByIdAsync(id);
        Assert.NotNull(saved);
        Assert.True(saved.Metadata.ContainsKey("TenantId"));
        Assert.Equal("tenant-1", saved.Metadata["TenantId"]);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDifferentTenant_ReturnsNull()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider("tenant-2");
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var space = new BehaviorSpace();
        space.SetMetadata("TenantId", "tenant-1");
        space.Observe(new BehaviorEvent("user", "a", DateTimeOffset.UtcNow));
        var id = await inner.SaveAsync(space);

        var result = await repo.GetByIdAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByMetadataAsync_WhenCurrentTenant_ReturnsOnlyMatchingSpaces()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider("tenant-1");
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));
        await repo.SaveAsync(space);

        var list = await repo.GetByMetadataAsync("TenantId", "tenant-1");

        Assert.NotEmpty(list);
        Assert.All(list, s => Assert.Equal("tenant-1", s.Metadata.GetValueOrDefault("TenantId")));
    }

    [Fact]
    public async Task GetByMetadataAsync_WhenTenantNull_DelegatesToInner()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider(null);
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("u", "a", DateTimeOffset.UtcNow));
        await inner.SaveAsync(space);

        var list = await repo.GetByMetadataAsync("TenantId", "any");

        Assert.Empty(list);
    }

    [Fact]
    public async Task GetByTimeWindowAsync_WhenCurrentTenant_ReturnsSpacesInWindow()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider("t1");
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var space = new BehaviorSpace();
        var t = DateTimeOffset.UtcNow;
        space.Observe(new BehaviorEvent("u", "a", t));
        await repo.SaveAsync(space);

        var list = await repo.GetByTimeWindowAsync(t.AddMinutes(-1), t.AddMinutes(1));

        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task GetByTimeWindowAsync_WhenTenantNull_DelegatesToInner()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider(null);
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var start = DateTimeOffset.UtcNow.AddHours(-1);
        var end = DateTimeOffset.UtcNow.AddHours(1);

        var list = await repo.GetByTimeWindowAsync(start, end);

        Assert.NotNull(list);
    }

    [Fact]
    public async Task DeleteAsync_WhenCurrentTenant_ReturnsTrue()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider("tenant-1");
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("u", "a", DateTimeOffset.UtcNow));
        var id = await repo.SaveAsync(space);

        var ok = await repo.DeleteAsync(id);

        Assert.True(ok);
        Assert.Null(await inner.GetByIdAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_WhenDifferentTenant_ReturnsFalse()
    {
        var inner = new TestBehaviorSpaceRepository();
        var provider = new FixedTenantProvider("tenant-2");
        var repo = new TenantAwareBehaviorSpaceRepository(inner, provider);
        var space = new BehaviorSpace();
        space.SetMetadata("TenantId", "tenant-1");
        space.Observe(new BehaviorEvent("u", "a", DateTimeOffset.UtcNow));
        var id = await inner.SaveAsync(space);

        var ok = await repo.DeleteAsync(id);

        Assert.False(ok);
        Assert.NotNull(await inner.GetByIdAsync(id));
    }

    private sealed class FixedTenantProvider : ITenantProvider
    {
        private readonly string? _id;
        public FixedTenantProvider(string? id) => _id = id;
        public string? GetCurrentTenantId() => _id;
    }

    private sealed class TestBehaviorSpaceRepository : IBehaviorSpaceRepository
    {
        private readonly Dictionary<string, BehaviorSpace> _store = new();

        public Task<string> SaveAsync(BehaviorSpace behaviorSpace, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            _store[id] = behaviorSpace;
            return Task.FromResult(id);
        }

        public Task<BehaviorSpace?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.GetValueOrDefault(id));

        public Task<IReadOnlyList<BehaviorSpace>> GetByMetadataAsync(string key, object value, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BehaviorSpace>>(_store.Values.Where(s => s.Metadata.TryGetValue(key, out var v) && Equals(v, value)).ToList());

        public Task<IReadOnlyList<BehaviorSpace>> GetByTimeWindowAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BehaviorSpace>>(_store.Values.ToList());

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var ok = _store.Remove(id);
            return Task.FromResult(ok);
        }
    }
}
