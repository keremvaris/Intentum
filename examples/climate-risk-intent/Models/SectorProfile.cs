namespace Intentum.Example.ClimateRisk.Models;

public sealed record SectorProfile(
    string Name,
    double PhysicalSensitivity,
    double TransitionSensitivity,
    IReadOnlyList<string> KeyRisks)
{
    public static readonly SectorProfile Energy = new(
        "Energy",
        PhysicalSensitivity: 0.7,
        TransitionSensitivity: 0.95,
        KeyRisks: ["Coal asset stranding", "Grid infrastructure investment", "Renewable transition cost"]);

    public static readonly SectorProfile Agriculture = new(
        "Agriculture",
        PhysicalSensitivity: 0.95,
        TransitionSensitivity: 0.4,
        KeyRisks: ["Yield loss", "Water stress", "Food price volatility"]);

    public static readonly SectorProfile RealEstate = new(
        "RealEstate",
        PhysicalSensitivity: 0.8,
        TransitionSensitivity: 0.5,
        KeyRisks: ["Coastal erosion", "Flood risk", "Insurance cost increase"]);

    public static readonly SectorProfile Finance = new(
        "Finance",
        PhysicalSensitivity: 0.3,
        TransitionSensitivity: 0.85,
        KeyRisks: ["Portfolio risk", "Credit losses", "Stress test exposure"]);

    public static readonly SectorProfile Tourism = new(
        "Tourism",
        PhysicalSensitivity: 0.75,
        TransitionSensitivity: 0.45,
        KeyRisks: ["Seasonal shift", "Natural disaster exposure", "Infrastructure damage"]);

    public static IReadOnlyList<SectorProfile> All => [Energy, Agriculture, RealEstate, Finance, Tourism];

    public static SectorProfile? GetById(string id) => All.FirstOrDefault(
        s => s.Name.Equals(id, StringComparison.OrdinalIgnoreCase));
}