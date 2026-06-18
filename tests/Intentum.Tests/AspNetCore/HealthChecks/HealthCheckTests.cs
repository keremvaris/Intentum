using Intentum.AI.Embeddings;
using Intentum.AspNetCore.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Xunit;

namespace Intentum.Tests.AspNetCore.HealthChecks;

public class HealthCheckTests
{
    [Fact]
    public async Task PolicyEngineHealthCheck_Healthy_ReturnsHealthy()
    {
        var check = new PolicyEngineHealthCheck();
        var context = new HealthCheckContext();
        
        var result = await check.CheckHealthAsync(context);
        
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
    
    [Fact]
    public async Task EmbeddingProviderHealthCheck_Healthy_ReturnsHealthy()
    {
        var provider = new Mock<IIntentEmbeddingProvider>();
        provider.Setup(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IntentEmbedding("test", 0.9, new double[] { 1, 0 }));
        
        var check = new EmbeddingProviderHealthCheck(provider.Object);
        var context = new HealthCheckContext();
        
        var result = await check.CheckHealthAsync(context);
        
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
    
    [Fact]
    public async Task EmbeddingProviderHealthCheck_Unhealthy_ReturnsUnhealthy()
    {
        var provider = new Mock<IIntentEmbeddingProvider>();
        provider.Setup(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));
        
        var check = new EmbeddingProviderHealthCheck(provider.Object);
        var context = new HealthCheckContext();
        
        var result = await check.CheckHealthAsync(context);
        
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}