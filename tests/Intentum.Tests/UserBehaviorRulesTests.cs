using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.UBA;

namespace Intentum.Tests;

/// <summary>
/// Tests for UserBehaviorRules: InsiderThreat, DataExfiltration, AnomalousAccess.
/// </summary>
public sealed class UserBehaviorRulesTests
{
    [Fact]
    public void InsiderThreat_WithBulkDownloadAndSensitiveAccess_ReturnsMatch()
    {
        var rule = UserBehaviorRules.InsiderThreat();
        var space = new BehaviorSpace()
            .Observe("user", "download")
            .Observe("user", "download")
            .Observe("user", "export")
            .Observe("user", "sensitive")
            .Observe("user", "admin");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("InsiderThreat", match.Name);
    }

    [Fact]
    public void InsiderThreat_WithPrivilegeEscalationAndSensitive_ReturnsMatch()
    {
        var rule = UserBehaviorRules.InsiderThreat();
        var space = new BehaviorSpace()
            .Observe("user", "privilege")
            .Observe("user", "confidential");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("InsiderThreat", match.Name);
    }

    [Fact]
    public void DataExfiltration_WithManyTransfers_ReturnsMatch()
    {
        var rule = UserBehaviorRules.DataExfiltration(minTransfers: 3);
        var space = new BehaviorSpace()
            .Observe("user", "transfer")
            .Observe("user", "upload")
            .Observe("user", "usb");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("DataExfiltration", match.Name);
    }

    [Fact]
    public void AnomalousAccess_WithOffHoursAndNewLocation_ReturnsMatch()
    {
        var rule = UserBehaviorRules.AnomalousAccess();
        var space = new BehaviorSpace()
            .Observe("user", "off-hours")
            .Observe("user", "new.location");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("AnomalousAccess", match.Name);
    }

    [Fact]
    public void AllRules_ReturnsThreeRules()
    {
        var rules = UserBehaviorRules.AllRules();

        Assert.Equal(3, rules.Count);
    }
}
