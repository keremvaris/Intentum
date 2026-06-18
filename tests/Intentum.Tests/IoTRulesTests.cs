using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.IoT;

namespace Intentum.Tests;

public sealed class IoTRulesTests
{
    [Fact]
    public void DeviceFailure_WithErrorAndTelemetryGap_ReturnsMatch()
    {
        var rule = IoTRules.DeviceFailure();
        var space = new BehaviorSpace()
            .Observe("device", "error.connection")
            .Observe("device", "telemetry.gap");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("DeviceFailure", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void DeviceFailure_WithNoEvents_ReturnsNull()
    {
        var rule = IoTRules.DeviceFailure();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void DeviceFailure_WithMultipleErrorsNoGap_ReturnsLowerConfidence()
    {
        var rule = IoTRules.DeviceFailure();
        var space = new BehaviorSpace()
            .Observe("device", "error.timeout")
            .Observe("device", "error.connection")
            .Observe("device", "error.protocol");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("DeviceFailure", match.Name);
        Assert.Equal(0.63, match.Score);
    }

    [Fact]
    public void SecurityBreach_WithTwoSignals_ReturnsMatch()
    {
        var rule = IoTRules.SecurityBreach();
        var space = new BehaviorSpace()
            .Observe("device", "access.unauthorized")
            .Observe("device", "location.anomaly");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("SecurityBreach", match.Name);
        Assert.Equal(0.95, match.Score);
    }

    [Fact]
    public void SecurityBreach_WithSingleSignal_ReturnsNull()
    {
        var rule = IoTRules.SecurityBreach();
        var space = new BehaviorSpace()
            .Observe("device", "access.unauthorized");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void MaintenanceRequired_WithTwoSignals_ReturnsMatch()
    {
        var rule = IoTRules.MaintenanceRequired();
        var space = new BehaviorSpace()
            .Observe("device", "performance.degrading")
            .Observe("device", "temperature.high");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("MaintenanceRequired", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void MaintenanceRequired_WithSingleSignal_ReturnsNull()
    {
        var rule = IoTRules.MaintenanceRequired();
        var space = new BehaviorSpace()
            .Observe("device", "temperature.high");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AnomalousSensorReading_WithTwoAnomalies_ReturnsMatch()
    {
        var rule = IoTRules.AnomalousSensorReading();
        var space = new BehaviorSpace()
            .Observe("sensor", "reading.outlier")
            .Observe("sensor", "reading.rapid.change");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AnomalousSensorReading", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void AnomalousSensorReading_WithSingleAnomaly_ReturnsNull()
    {
        var rule = IoTRules.AnomalousSensorReading();
        var space = new BehaviorSpace()
            .Observe("sensor", "reading.outlier");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void FirmwareOutdated_WithSingleSignal_ReturnsMatch()
    {
        var rule = IoTRules.FirmwareOutdated();
        var space = new BehaviorSpace()
            .Observe("device", "firmware.version.mismatch");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("FirmwareOutdated", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void FirmwareOutdated_WithNoSignals_ReturnsNull()
    {
        var rule = IoTRules.FirmwareOutdated();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void NetworkCongestion_WithTwoSignals_ReturnsMatch()
    {
        var rule = IoTRules.NetworkCongestion();
        var space = new BehaviorSpace()
            .Observe("device", "network.latency.spike")
            .Observe("device", "network.packet.loss");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("NetworkCongestion", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void NetworkCongestion_WithSingleSignal_ReturnsNull()
    {
        var rule = IoTRules.NetworkCongestion();
        var space = new BehaviorSpace()
            .Observe("device", "network.latency.spike");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void PowerFluctuation_WithSingleSignal_ReturnsMatch()
    {
        var rule = IoTRules.PowerFluctuation();
        var space = new BehaviorSpace()
            .Observe("device", "power.voltage.anomaly");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("PowerFluctuation", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void PowerFluctuation_WithNoSignals_ReturnsNull()
    {
        var rule = IoTRules.PowerFluctuation();
        var space = new BehaviorSpace();

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void ConfigurationDrift_WithTwoSignals_ReturnsMatch()
    {
        var rule = IoTRules.ConfigurationDrift();
        var space = new BehaviorSpace()
            .Observe("device", "config.setting.changed")
            .Observe("device", "config.compliance.violation");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("ConfigurationDrift", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void ConfigurationDrift_WithSingleSignal_ReturnsNull()
    {
        var rule = IoTRules.ConfigurationDrift();
        var space = new BehaviorSpace()
            .Observe("device", "config.setting.changed");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AllRules_ReturnsEightRules()
    {
        var rules = IoTRules.AllRules();

        Assert.Equal(8, rules.Count);
    }
}
