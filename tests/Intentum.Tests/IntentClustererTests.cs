using Intentum.Clustering;
using Intentum.Persistence.Repositories;
using Intentum.Runtime.Policy;
using Microsoft.Extensions.DependencyInjection;

namespace Intentum.Tests;

public class IntentClustererTests
{
    [Fact]
    public async Task ClusterByPatternAsync_EmptyRecords_ReturnsEmpty()
    {
        var clusterer = new IntentClusterer();
        var clusters = await clusterer.ClusterByPatternAsync([]);
        Assert.Empty(clusters);
    }

    [Fact]
    public async Task ClusterByPatternAsync_Records_GroupsByConfidenceAndDecision()
    {
        var records = new List<IntentHistoryRecord>
        {
            CreateRecord("1", "High", PolicyDecision.Allow),
            CreateRecord("2", "High", PolicyDecision.Allow),
            CreateRecord("3", "Low", PolicyDecision.Block)
        };
        var clusterer = new IntentClusterer();
        var clusters = await clusterer.ClusterByPatternAsync(records);
        Assert.Equal(2, clusters.Count);
        Assert.Contains(clusters, c => c.Count == 2 && c.Label.Contains("High"));
        Assert.Contains(clusters, c => c.Count == 1 && c.Label.Contains("Low"));
    }

    [Fact]
    public async Task ClusterByPatternAsync_ClusterExposesIdRecordIdsAndSummary()
    {
        var records = new List<IntentHistoryRecord>
        {
            CreateRecord("r1", "High", PolicyDecision.Allow, 0.9),
            CreateRecord("r2", "High", PolicyDecision.Allow, 0.85)
        };
        var clusterer = new IntentClusterer();
        var clusters = await clusterer.ClusterByPatternAsync(records);
        var cluster = Assert.Single(clusters);
        Assert.Equal("High_Allow", cluster.Id);
        Assert.Equal(2, cluster.RecordIds.Count);
        Assert.Contains("r1", cluster.RecordIds);
        Assert.Contains("r2", cluster.RecordIds);
        Assert.NotNull(cluster.Summary);
        Assert.InRange(cluster.Summary.AverageConfidenceScore, 0.8, 1.0);
        Assert.Equal(0.85, cluster.Summary.MinScore);
        Assert.Equal(0.9, cluster.Summary.MaxScore);
    }

    [Fact]
    public async Task ClusterByConfidenceScoreAsync_SplitsIntoKBuckets()
    {
        var records = new List<IntentHistoryRecord>
        {
            CreateRecord("1", "High", PolicyDecision.Allow, 0.9),
            CreateRecord("2", "Medium", PolicyDecision.Observe),
            CreateRecord("3", "Low", PolicyDecision.Block, 0.2)
        };
        var clusterer = new IntentClusterer();
        var clusters = await clusterer.ClusterByConfidenceScoreAsync(records, k: 3);
        Assert.Equal(3, clusters.Count);
    }

    [Fact]
    public void AddIntentClustering_RegistersIntentClusterer()
    {
        var services = new ServiceCollection();
        services.AddIntentClustering();
        var provider = services.BuildServiceProvider();
        var clusterer = provider.GetService<IIntentClusterer>();
        Assert.NotNull(clusterer);
        Assert.IsType<IntentClusterer>(clusterer);
    }

    private static IntentHistoryRecord CreateRecord(string id, string level, PolicyDecision decision, double score = 0.5)
        => new(id, "bs1", "Intent", level, score, decision, DateTimeOffset.UtcNow);
}
