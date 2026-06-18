using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.Finance;

public static class FinanceRules
{
    public static Func<BehaviorSpace, RuleMatch?> MoneyLaunderingPattern(
        double confidence = 0.9) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("transfer.rapid", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("structuring.detected", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("jurisdiction.high_risk", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("transfer.round_amounts", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("MoneyLaunderingPattern", confidence,
                $"Money laundering signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> UnauthorizedAccess(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("login.unusual.time", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("device.new", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("login.failed.multiple", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("location.unusual", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("UnauthorizedAccess", confidence,
                $"Unauthorized access signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> HighValueTransaction(
        double confidence = 0.75) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("transaction.high_value", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("recipient.unusual", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("transaction.rapid_sequence", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("HighValueTransaction", confidence,
                $"High-value transaction signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> AccountCompromise(
        double confidence = 0.95) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("password.changed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("profile.updated", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("login.suspicious", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("mfa.disabled", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("AccountCompromise", confidence,
                $"Account compromise signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> InsiderTrading(
        double confidence = 0.9) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("trade.unusual.pattern", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("trade.pre.announcement", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("trade.timing.suspicious", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("trade.volume.anomaly", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("InsiderTrading", confidence,
                $"Insider trading signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> CreditFraud(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("credit.application.rapid", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("identity.mismatch", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("credit.fabricated.documents", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("credit.multiple.inquiries", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("CreditFraud", confidence,
                $"Credit fraud signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> WireFraud(
        double confidence = 0.9) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("wire.unusual.pattern", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("wire.beneficiary.changed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("wire.urgent.request", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("wire.account.mismatch", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("WireFraud", confidence,
                $"Wire fraud signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> ComplianceViolation(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("compliance.regulatory.flag", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("compliance.reporting.gap", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("compliance.limit.exceeded", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("compliance.approval.missing", StringComparison.OrdinalIgnoreCase));

        if (signals >= 1)
            return new RuleMatch("ComplianceViolation", confidence,
                $"Compliance violation signals: {signals}");
        return null;
    };

    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        AccountCompromise(),
        MoneyLaunderingPattern(),
        UnauthorizedAccess(),
        HighValueTransaction(),
        InsiderTrading(),
        CreditFraud(),
        WireFraud(),
        ComplianceViolation()
    ];
}
