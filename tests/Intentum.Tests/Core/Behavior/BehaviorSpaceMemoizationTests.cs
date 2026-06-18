using Intentum.Core.Behavior;

namespace Intentum.Tests.Core.Behavior;

public class BehaviorSpaceMemoizationTests
{
    [Fact]
    public void ToVector_WithSameOptions_ReturnsCachedResult()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));
        var options = new ToVectorOptions { Normalization = VectorNormalization.L1 };
        
        var vector1 = space.ToVector(options);
        var vector2 = space.ToVector(options);
        
        Assert.Same(vector1, vector2);
    }
    
    [Fact]
    public void ToVector_WithDifferentOptions_ReturnsDifferentResults()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));
        var options1 = new ToVectorOptions { Normalization = VectorNormalization.L1 };
        var options2 = new ToVectorOptions { Normalization = VectorNormalization.Cap, CapPerDimension = 5 };
        
        var vector1 = space.ToVector(options1);
        var vector2 = space.ToVector(options2);
        
        Assert.NotSame(vector1, vector2);
    }
    
    [Fact]
    public void ToVector_AfterNewEvent_InvalidatesCache()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));
        var vector1 = space.ToVector();
        
        space.Observe(new BehaviorEvent("user", "logout", DateTimeOffset.UtcNow));
        var vector2 = space.ToVector();
        
        Assert.NotSame(vector1, vector2);
        Assert.Equal(2, vector2.Dimensions.Count);
    }
    
    [Fact]
    public void ToVector_NullOptions_CachesByDefault()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));
        
        var vector1 = space.ToVector();
        var vector2 = space.ToVector();
        
        Assert.Same(vector1, vector2);
    }
}
