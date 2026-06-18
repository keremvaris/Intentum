using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Runtime.Policy;
using Moq;

namespace Intentum.Tests.Core.Behavior;

public class BehaviorSpaceValidationTests
{
    [Fact]
    public void InferWithValidation_WithEmptySpace_ThrowsArgumentException()
    {
        var space = new BehaviorSpace();
        var model = new Mock<IIntentModel>();

        var ex = Assert.Throws<ArgumentException>(() => model.Object.InferWithValidation(space));
        Assert.Contains("at least one", ex.Message.ToLower());
    }

    [Fact]
    public void InferWithValidation_WithEvents_CallsModel()
    {
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", DateTimeOffset.UtcNow));
        var model = new Mock<IIntentModel>();
        model.Setup(m => m.Infer(space, null)).Returns(
            new Intent("Test", [], new IntentConfidence(0.5, "Medium")));

        var result = model.Object.InferWithValidation(space);

        Assert.NotNull(result);
        model.Verify(m => m.Infer(space, null), Times.Once);
    }

    [Fact]
    public void InferWithValidation_WithNullSpace_ThrowsArgumentNullException()
    {
        var model = new Mock<IIntentModel>();

        Assert.Throws<ArgumentNullException>(() => model.Object.InferWithValidation(null!));
    }

    [Fact]
    public void IntentPolicy_Validate_WithNoRules_ThrowsInvalidOperationException()
    {
        var policy = new IntentPolicy();

        Assert.Throws<InvalidOperationException>(() => policy.Validate());
    }

    [Fact]
    public void IntentPolicy_Validate_WithRules_DoesNotThrow()
    {
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule("Test", _ => true, PolicyDecision.Allow));

        var exception = Record.Exception(() => policy.Validate());

        Assert.Null(exception);
    }

    [Fact]
    public void IntentPolicy_Validate_WithDuplicateNames_ThrowsInvalidOperationException()
    {
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule("Duplicate", _ => true, PolicyDecision.Allow))
            .AddRule(new PolicyRule("Duplicate", _ => true, PolicyDecision.Block));

        Assert.Throws<InvalidOperationException>(() => policy.Validate());
    }
}
