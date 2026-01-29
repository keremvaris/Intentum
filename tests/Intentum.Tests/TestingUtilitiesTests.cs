using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;
using Intentum.Testing;

namespace Intentum.Tests;

/// <summary>
/// Tests demonstrating the usage of testing utilities.
/// </summary>
public class TestingUtilitiesTests
{
    [Fact]
    public void TestHelpers_CreateDefaultModel_Works()
    {
        // Arrange & Act
        var model = TestHelpers.CreateDefaultModel();
        var space = TestHelpers.CreateSimpleSpace();

        // Act
        var intent = model.Infer(space);

        // Assert
        Assert.NotNull(intent);
        IntentAssertions.HasSignals(intent);
    }

    [Fact]
    public void TestHelpers_CreateDefaultPolicy_Works()
    {
        // Arrange & Act
        var policy = TestHelpers.CreateDefaultPolicy();

        // Assert
        Assert.Equal(4, policy.Rules.Count);
    }

    [Fact]
    public void TestHelpers_CreateSimpleSpace_Works()
    {
        // Arrange & Act
        var space = TestHelpers.CreateSimpleSpace();
        Assert.NotNull(space);
        // Assert
        BehaviorSpaceAssertions.HasEventCount(space, 2);
        BehaviorSpaceAssertions.ContainsActor(space, "user");
        BehaviorSpaceAssertions.ContainsAction(space, "login");
        BehaviorSpaceAssertions.ContainsAction(space, "submit");
    }

    [Fact]
    public void TestHelpers_CreateSpaceWithRetries_Works()
    {
        // Arrange & Act
        var space = TestHelpers.CreateSpaceWithRetries(3);
        Assert.NotNull(space);
        // Assert
        BehaviorSpaceAssertions.HasEventCount(space, 5); // login + 3 retries + submit
        BehaviorSpaceAssertions.ContainsAction(space, "retry");
    }

    [Fact]
    public void BehaviorSpaceAssertions_ContainsEvent_Works()
    {
        // Arrange
        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
            .Build();
        Assert.NotNull(space);
        // Act & Assert
        BehaviorSpaceAssertions.ContainsEvent(space, "user", "login");
    }

    [Fact]
    public void IntentAssertions_HasConfidenceLevel_Works()
    {
        // Arrange
        var model = TestHelpers.CreateDefaultModel();
        var space = TestHelpers.CreateSimpleSpace();
        var intent = model.Infer(space);
        Assert.NotNull(intent);
        // Act & Assert
        IntentAssertions.HasConfidenceLevel(intent, intent.Confidence.Level);
        IntentAssertions.HasConfidenceScore(intent, 0.0, 1.0);
    }

    [Fact]
    public void PolicyDecisionAssertions_IsOneOf_Works()
    {
        // Arrange
        var model = TestHelpers.CreateDefaultModel();
        var space = TestHelpers.CreateSimpleSpace();
        var policy = TestHelpers.CreateDefaultPolicy();
        // Act
        var intent = model.Infer(space);
        Assert.NotNull(intent);
        var decision = intent.Decide(policy);
        // Assert (default policy may return Allow, Observe, or Warn depending on inferred intent)
        PolicyDecisionAssertions.IsOneOf(decision, PolicyDecision.Allow, PolicyDecision.Observe, PolicyDecision.Warn);
    }
}
