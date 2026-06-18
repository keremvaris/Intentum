using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Finance;

namespace Intentum.Tests;

public sealed class FinanceRulesTests
{
    [Fact]
    public void MoneyLaunderingPattern_WithTwoSignals_ReturnsMatch()
    {
        var rule = FinanceRules.MoneyLaunderingPattern();
        var space = new BehaviorSpace()
            .Observe("system", "transfer.rapid")
            .Observe("system", "structuring.detected");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("MoneyLaunderingPattern", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void MoneyLaunderingPattern_WithSingleSignal_ReturnsNull()
    {
        var rule = FinanceRules.MoneyLaunderingPattern();
        var space = new BehaviorSpace()
            .Observe("system", "transfer.rapid");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void UnauthorizedAccess_WithTwoSignals_ReturnsMatch()
    {
        var rule = FinanceRules.UnauthorizedAccess();
        var space = new BehaviorSpace()
            .Observe("system", "login.unusual.time")
            .Observe("system", "device.new");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("UnauthorizedAccess", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void UnauthorizedAccess_WithSingleSignal_ReturnsNull()
    {
        var rule = FinanceRules.UnauthorizedAccess();
        var space = new BehaviorSpace()
            .Observe("system", "login.unusual.time");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void HighValueTransaction_WithTwoSignals_ReturnsMatch()
    {
        var rule = FinanceRules.HighValueTransaction();
        var space = new BehaviorSpace()
            .Observe("system", "transaction.high_value")
            .Observe("system", "recipient.unusual");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("HighValueTransaction", match.Name);
        Assert.Equal(0.75, match.Score);
    }

    [Fact]
    public void HighValueTransaction_WithSingleSignal_ReturnsNull()
    {
        var rule = FinanceRules.HighValueTransaction();
        var space = new BehaviorSpace()
            .Observe("system", "transaction.high_value");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AccountCompromise_WithTwoSignals_ReturnsMatch()
    {
        var rule = FinanceRules.AccountCompromise();
        var space = new BehaviorSpace()
            .Observe("system", "password.changed")
            .Observe("system", "login.suspicious");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AccountCompromise", match.Name);
        Assert.Equal(0.95, match.Score);
    }

    [Fact]
    public void AccountCompromise_WithSingleSignal_ReturnsNull()
    {
        var rule = FinanceRules.AccountCompromise();
        var space = new BehaviorSpace()
            .Observe("system", "password.changed");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void InsiderTrading_WithTwoSignals_ReturnsMatch()
    {
        var rule = FinanceRules.InsiderTrading();
        var space = new BehaviorSpace()
            .Observe("system", "trade.unusual.pattern")
            .Observe("system", "trade.pre.announcement");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("InsiderTrading", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void InsiderTrading_WithSingleSignal_ReturnsNull()
    {
        var rule = FinanceRules.InsiderTrading();
        var space = new BehaviorSpace()
            .Observe("system", "trade.unusual.pattern");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void CreditFraud_WithTwoSignals_ReturnsMatch()
    {
        var rule = FinanceRules.CreditFraud();
        var space = new BehaviorSpace()
            .Observe("system", "credit.application.rapid")
            .Observe("system", "identity.mismatch");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("CreditFraud", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void CreditFraud_WithSingleSignal_ReturnsNull()
    {
        var rule = FinanceRules.CreditFraud();
        var space = new BehaviorSpace()
            .Observe("system", "credit.application.rapid");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void WireFraud_WithTwoSignals_ReturnsMatch()
    {
        var rule = FinanceRules.WireFraud();
        var space = new BehaviorSpace()
            .Observe("system", "wire.unusual.pattern")
            .Observe("system", "wire.beneficiary.changed");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("WireFraud", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void WireFraud_WithSingleSignal_ReturnsNull()
    {
        var rule = FinanceRules.WireFraud();
        var space = new BehaviorSpace()
            .Observe("system", "wire.unusual.pattern");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void ComplianceViolation_WithSingleSignal_ReturnsMatch()
    {
        var rule = FinanceRules.ComplianceViolation();
        var space = new BehaviorSpace()
            .Observe("system", "compliance.regulatory.flag");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("ComplianceViolation", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void ComplianceViolation_WithNoSignals_ReturnsNull()
    {
        var rule = FinanceRules.ComplianceViolation();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AllRules_ReturnsEightRules()
    {
        var rules = FinanceRules.AllRules();

        Assert.Equal(8, rules.Count);
    }
}
