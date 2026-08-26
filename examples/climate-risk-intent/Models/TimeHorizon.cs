namespace Intentum.Example.ClimateRisk.Models;

public enum TimeHorizon
{
    NearTerm2030 = 2030,
    MediumTerm2050 = 2050,
    LongTerm2100 = 2100
}

public static class TimeHorizonExtensions
{
    public static double GetMultiplier(this TimeHorizon horizon) => horizon switch
    {
        TimeHorizon.NearTerm2030 => 0.3,
        TimeHorizon.MediumTerm2050 => 0.65,
        TimeHorizon.LongTerm2100 => 1.0,
        _ => 0.5
    };

    public static string GetDescription(this TimeHorizon horizon) => horizon switch
    {
        TimeHorizon.NearTerm2030 => "Short-term (2030): Immediate policy and investment impacts",
        TimeHorizon.MediumTerm2050 => "Medium-term (2050): Significant physical and transition effects",
        TimeHorizon.LongTerm2100 => "Long-term (2100): Full climate impact realization",
        _ => "Unknown horizon"
    };
}