using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.Fraud;

/// <summary>
/// Pre-built fraud detection rules for common attack patterns.
/// Use with RuleBasedIntentModel or ChainedIntentModel.
/// </summary>
public static class FraudRules
{
    /// <summary>
    /// Detects account takeover attempts: multiple failed logins + password reset + IP/device change.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> AccountTakeover(
        int minFailedLogins = 3,
        double confidence = 0.9) => space =>
    {
        var failedLogins = space.Events.Count(e =>
            e.Action.Contains("login.failed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("FailedLogin", StringComparison.OrdinalIgnoreCase));
        var hasIpChange = space.Events.Any(e =>
            e.Action.Contains("ip.changed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("device.new", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("NewDevice", StringComparison.OrdinalIgnoreCase));
        var hasPasswordReset = space.Events.Any(e =>
            e.Action.Contains("password.reset", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("PasswordReset", StringComparison.OrdinalIgnoreCase));

        if (failedLogins >= minFailedLogins && (hasIpChange || hasPasswordReset))
            return new RuleMatch("AccountTakeover", confidence,
                $"Failed logins: {failedLogins}, IP change: {hasIpChange}, password reset: {hasPasswordReset}");
        return null;
    };

    /// <summary>
    /// Detects credential stuffing: high-frequency login attempts from different actors.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> CredentialStuffing(
        int minAttempts = 5,
        double confidence = 0.85) => space =>
    {
        var loginAttempts = space.Events.Count(e =>
            e.Action.Contains("login", StringComparison.OrdinalIgnoreCase));
        var uniqueActors = space.Events.Select(e => e.Actor).Distinct().Count();

        if (loginAttempts >= minAttempts && uniqueActors >= 3)
            return new RuleMatch("CredentialStuffing", confidence,
                $"Login attempts: {loginAttempts} from {uniqueActors} actors");
        return null;
    };

    /// <summary>
    /// Detects payment fraud: rapid high-value transactions, card testing patterns.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> PaymentFraud(
        int minTransactions = 3,
        double confidence = 0.8) => space =>
    {
        var payments = space.Events.Count(e =>
            e.Action.Contains("payment", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("transaction", StringComparison.OrdinalIgnoreCase));
        var declines = space.Events.Count(e =>
            e.Action.Contains("declined", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("failed", StringComparison.OrdinalIgnoreCase));

        if (payments >= minTransactions && declines >= 2)
            return new RuleMatch("PaymentFraud", confidence,
                $"Payments: {payments}, declines: {declines}");
        return null;
    };

    /// <summary>
    /// Detects legitimate account recovery: failed login + password reset + successful login.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> AccountRecovery(
        double confidence = 0.85) => space =>
    {
        var hasFailed = space.Events.Any(e =>
            e.Action.Contains("login.failed", StringComparison.OrdinalIgnoreCase));
        var hasReset = space.Events.Any(e =>
            e.Action.Contains("password.reset", StringComparison.OrdinalIgnoreCase));
        var hasSuccess = space.Events.Any(e =>
            e.Action.Contains("login.success", StringComparison.OrdinalIgnoreCase));

        if (hasFailed && hasReset && hasSuccess)
            return new RuleMatch("AccountRecovery", confidence,
                "Pattern: failed login -> password reset -> successful login");
        return null;
    };

    /// <summary>
    /// Returns all standard fraud detection rules in recommended evaluation order.
    /// </summary>
    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        AccountTakeover(),
        CredentialStuffing(),
        PaymentFraud(),
        AccountRecovery()
    ];
}
