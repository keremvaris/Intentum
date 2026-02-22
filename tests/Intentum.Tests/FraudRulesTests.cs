using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Fraud;

namespace Intentum.Tests;

/// <summary>
/// Tests for FraudRules: AccountTakeover, CredentialStuffing, PaymentFraud, AccountRecovery.
/// </summary>
public sealed class FraudRulesTests
{
    [Fact]
    public void AccountTakeover_WithFailedLoginsAndPasswordReset_ReturnsMatch()
    {
        var rule = FraudRules.AccountTakeover(minFailedLogins: 3);
        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "login.failed")
            .Observe("user", "login.failed")
            .Observe("user", "password.reset");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AccountTakeover", match.Name);
    }

    [Fact]
    public void AccountTakeover_WithIpChange_ReturnsMatch()
    {
        var rule = FraudRules.AccountTakeover(minFailedLogins: 2);
        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "login.failed")
            .Observe("user", "ip.changed");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AccountTakeover", match.Name);
    }

    [Fact]
    public void AccountTakeover_WithInsufficientFailedLogins_ReturnsNull()
    {
        var rule = FraudRules.AccountTakeover(minFailedLogins: 3);
        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "password.reset");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void CredentialStuffing_WithManyLoginsFromMultipleActors_ReturnsMatch()
    {
        var rule = FraudRules.CredentialStuffing(minAttempts: 5);
        var space = new BehaviorSpace()
            .Observe("actor1", "login")
            .Observe("actor2", "login")
            .Observe("actor3", "login")
            .Observe("actor1", "login")
            .Observe("actor2", "login");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("CredentialStuffing", match.Name);
    }

    [Fact]
    public void PaymentFraud_WithPaymentsAndDeclines_ReturnsMatch()
    {
        var rule = FraudRules.PaymentFraud(minTransactions: 3);
        var space = new BehaviorSpace()
            .Observe("user", "payment")
            .Observe("user", "transaction")
            .Observe("user", "payment")
            .Observe("user", "declined")
            .Observe("user", "failed");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("PaymentFraud", match.Name);
    }

    [Fact]
    public void AccountRecovery_WithFailedResetSuccessPattern_ReturnsMatch()
    {
        var rule = FraudRules.AccountRecovery();
        var space = new BehaviorSpace()
            .Observe("user", "login.failed")
            .Observe("user", "password.reset")
            .Observe("user", "login.success");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AccountRecovery", match.Name);
    }

    [Fact]
    public void AllRules_ReturnsFourRules()
    {
        var rules = FraudRules.AllRules();

        Assert.Equal(4, rules.Count);
    }
}
