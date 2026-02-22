using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.UBA;

/// <summary>
/// Pre-built User Behavior Analytics (UBA) rules for detecting insider threats,
/// anomalous access patterns, and data exfiltration attempts.
/// </summary>
public static class UserBehaviorRules
{
    /// <summary>
    /// Detects potential insider threat: off-hours access + bulk data download + privilege escalation.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> InsiderThreat(
        double confidence = 0.85) => space =>
    {
        var bulkDownload = space.Events.Count(e =>
            e.Action.Contains("download", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("export", StringComparison.OrdinalIgnoreCase));
        var sensitiveAccess = space.Events.Count(e =>
            e.Action.Contains("sensitive", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("confidential", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("admin", StringComparison.OrdinalIgnoreCase));
        var hasPrivilegeEscalation = space.Events.Any(e =>
            e.Action.Contains("privilege", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("role.change", StringComparison.OrdinalIgnoreCase));

        if (bulkDownload >= 3 && sensitiveAccess >= 2)
            return new RuleMatch("InsiderThreat", confidence,
                $"Bulk downloads: {bulkDownload}, sensitive access: {sensitiveAccess}, privilege escalation: {hasPrivilegeEscalation}");
        if (hasPrivilegeEscalation && sensitiveAccess >= 1)
            return new RuleMatch("InsiderThreat", confidence * 0.9,
                $"Privilege escalation with sensitive access: {sensitiveAccess}");
        return null;
    };

    /// <summary>
    /// Detects data exfiltration: large file transfers, USB usage, email attachments.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> DataExfiltration(
        int minTransfers = 3,
        double confidence = 0.8) => space =>
    {
        var transfers = space.Events.Count(e =>
            e.Action.Contains("transfer", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("upload", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("usb", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("attachment", StringComparison.OrdinalIgnoreCase));

        if (transfers >= minTransfers)
            return new RuleMatch("DataExfiltration", confidence,
                $"Suspicious transfers: {transfers}");
        return null;
    };

    /// <summary>
    /// Detects anomalous access patterns: unusual time, location, or resource access.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> AnomalousAccess(
        double confidence = 0.7) => space =>
    {
        var offHoursAccess = space.Events.Any(e =>
            e.Action.Contains("off-hours", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("weekend", StringComparison.OrdinalIgnoreCase));
        var newLocationAccess = space.Events.Any(e =>
            e.Action.Contains("new.location", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("vpn.new", StringComparison.OrdinalIgnoreCase));
        var unusualResource = space.Events.Any(e =>
            e.Action.Contains("first.access", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("unusual.resource", StringComparison.OrdinalIgnoreCase));

        var anomalyCount = (offHoursAccess ? 1 : 0) + (newLocationAccess ? 1 : 0) + (unusualResource ? 1 : 0);
        if (anomalyCount >= 2)
            return new RuleMatch("AnomalousAccess", confidence,
                $"Off-hours: {offHoursAccess}, new location: {newLocationAccess}, unusual resource: {unusualResource}");
        return null;
    };

    /// <summary>
    /// Returns all standard UBA rules in recommended evaluation order.
    /// </summary>
    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        InsiderThreat(),
        DataExfiltration(),
        AnomalousAccess()
    ];
}
