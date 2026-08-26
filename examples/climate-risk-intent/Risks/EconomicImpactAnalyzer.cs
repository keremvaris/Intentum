using Intentum.Example.ClimateRisk.Models;

namespace Intentum.Example.ClimateRisk.Risks;

public static class EconomicImpactAnalyzer
{
    public static EconomicImpact Calculate(
        double physicalRisk,
        double transitionRisk,
        SectorProfile sector)
    {
        var physicalEtd = physicalRisk * sector.PhysicalSensitivity;
        var transitionEtd = transitionRisk * sector.TransitionSensitivity;

        var gdpImpact = -(physicalEtd * 0.05 + transitionEtd * 0.03);
        var investmentImpact = transitionEtd * 0.2 + physicalEtd * 0.1;
        var insuranceCost = physicalEtd * 0.4;
        var borrowingCost = transitionEtd * 0.15 + physicalEtd * 0.1;
        var workforceImpact = -(physicalEtd * 0.02 + transitionEtd * 0.015);

        var totalScore = (physicalRisk + transitionRisk) / 2.0;

        return new EconomicImpact(
            GdpImpactPercent: gdpImpact,
            InvestmentImpactPercent: investmentImpact,
            InsuranceCostIncreasePercent: insuranceCost,
            BorrowingCostIncreasePercent: borrowingCost,
            WorkforceImpactPercent: workforceImpact,
            TotalScore: totalScore);
    }
}

public sealed record EconomicImpact(
    double GdpImpactPercent,
    double InvestmentImpactPercent,
    double InsuranceCostIncreasePercent,
    double BorrowingCostIncreasePercent,
    double WorkforceImpactPercent,
    double TotalScore)
{
    public string Summary =>
        $"GDP: {GdpImpactPercent:+0.0%;-0.0%} | " +
        $"CAPEX: +{InvestmentImpactPercent:P0} | " +
        $"Insurance: +{InsuranceCostIncreasePercent:P0} | " +
        $"Borrowing: +{BorrowingCostIncreasePercent:P0}";
}
