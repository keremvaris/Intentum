namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

public sealed class FinancialImpactEngine
{
    public FinancialImpact Calculate(
        CompanyProfile profile,
        double physicalRisk,
        double transitionRisk,
        IReadOnlyCollection<string> activeSignals)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(activeSignals);

        var lineImpacts = new List<LineItemImpact>();
        var categoryImpacts = new List<CategoryImpact>();

        foreach (var category in profile.Categories)
        {
            var catPhysical = 0.0;
            var catTransition = 0.0;

            foreach (var item in category.LineItems)
            {
                var boost = ComputeSignalBoost(item.MappedRiskSignals, activeSignals);
                var physical = item.Value * physicalRisk * item.PhysicalSensitivity * boost;
                var transition = item.Value * transitionRisk * item.TransitionSensitivity * boost;

                var signedPhysical = SignByCategory(physical, category.Type);
                var signedTransition = SignByCategory(transition, category.Type);

                lineImpacts.Add(new LineItemImpact
                {
                    CategoryId = category.Id,
                    LineItemId = item.Id,
                    Name = item.Name,
                    PhysicalImpact = signedPhysical,
                    TransitionImpact = signedTransition
                });

                catPhysical += signedPhysical;
                catTransition += signedTransition;
            }

            categoryImpacts.Add(new CategoryImpact
            {
                CategoryId = category.Id,
                Name = category.Name,
                Type = category.Type,
                PhysicalImpact = catPhysical,
                TransitionImpact = catTransition
            });
        }

        return new FinancialImpact
        {
            LineItemImpacts = lineImpacts,
            CategoryImpacts = categoryImpacts
        };
    }

    private static double ComputeSignalBoost(IReadOnlyCollection<string> mapped, IReadOnlyCollection<string> active)
    {
        if (mapped.Count == 0 || active.Count == 0) return 1.0;
        var matched = mapped.Count(active.Contains);
        return 1.0 + 0.2 * ((double)matched / mapped.Count);
    }

    private static double SignByCategory(double value, FinancialCategoryType type) =>
        type is FinancialCategoryType.Revenue or FinancialCategoryType.CashFlow ? -value : value;
}
