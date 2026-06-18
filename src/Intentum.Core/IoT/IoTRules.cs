using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.IoT;

public static class IoTRules
{
    public static Func<BehaviorSpace, RuleMatch?> DeviceFailure(
        double confidence = 0.9) => space =>
    {
        var errors = space.Events.Count(e =>
            e.Action.Contains("error", StringComparison.OrdinalIgnoreCase));
        var gaps = space.Events.Count(e =>
            e.Action.Contains("telemetry.gap", StringComparison.OrdinalIgnoreCase));

        if (errors >= 1 && gaps >= 1)
            return new RuleMatch("DeviceFailure", confidence,
                $"Errors: {errors}, telemetry gaps: {gaps}");
        if (errors >= 3)
            return new RuleMatch("DeviceFailure", confidence * 0.7,
                $"Multiple errors: {errors}, no gaps detected");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> SecurityBreach(
        double confidence = 0.95) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("access.unauthorized", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("location.anomaly", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("firmware.unauthorized", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("port.scan.detected", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("SecurityBreach", confidence,
                $"Security signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> MaintenanceRequired(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("performance.degrading", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("usage.threshold.exceeded", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("temperature.high", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("battery.low", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("MaintenanceRequired", confidence,
                $"Maintenance signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> AnomalousSensorReading(
        double confidence = 0.8) => space =>
    {
        var anomalies = space.Events.Count(e =>
            e.Action.Contains("reading.outlier", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("reading.rapid.change", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("reading.calibration.off", StringComparison.OrdinalIgnoreCase));

        if (anomalies >= 2)
            return new RuleMatch("AnomalousSensorReading", confidence,
                $"Anomalous readings: {anomalies}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> FirmwareOutdated(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("firmware.version.mismatch", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("firmware.update.failed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("firmware.deprecated", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("security.patch.missing", StringComparison.OrdinalIgnoreCase));

        if (signals >= 1)
            return new RuleMatch("FirmwareOutdated", confidence,
                $"Firmware signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> NetworkCongestion(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("network.latency.spike", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("network.packet.loss", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("network.throughput.drop", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("network.connection.intermittent", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("NetworkCongestion", confidence,
                $"Network signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> PowerFluctuation(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("power.voltage.anomaly", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("power.battery.drain", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("power.surge.detected", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("power.brownout", StringComparison.OrdinalIgnoreCase));

        if (signals >= 1)
            return new RuleMatch("PowerFluctuation", confidence,
                $"Power signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> ConfigurationDrift(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("config.setting.changed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("config.compliance.violation", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("config.unexpected.change", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("config.rollback", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("ConfigurationDrift", confidence,
                $"Configuration signals: {signals}");
        return null;
    };

    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        SecurityBreach(),
        DeviceFailure(),
        MaintenanceRequired(),
        AnomalousSensorReading(),
        FirmwareOutdated(),
        NetworkCongestion(),
        PowerFluctuation(),
        ConfigurationDrift()
    ];
}
