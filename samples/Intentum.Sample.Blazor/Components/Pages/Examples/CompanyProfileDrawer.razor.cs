using Intentum.Sample.Blazor.Components.Services.ClimateData;
using Microsoft.AspNetCore.Components;

namespace Intentum.Sample.Blazor.Components.Pages.Examples;

public partial class CompanyProfileDrawer : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public CompanyProfile? Profile { get; set; }
    [Parameter] public bool IsNew { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public EventCallback<CompanyProfile> OnSave { get; set; }

    private CompanyProfile editProfile = new();

    protected override void OnParametersSet()
    {
        if (Profile != null)
        {
            editProfile = Profile.DeepClone();
        }
        else
        {
            editProfile = new CompanyProfile
            {
                Name = "",
                Sector = "Sanayi",
                LocationName = "",
                Categories = new List<FinancialCategory>
                {
                    new() { Type = FinancialCategoryType.Revenue, Name = "Gelir", LineItems = new List<FinancialLineItem>
                    {
                        new() { Name = "Ana gelir", Value = 50_000_000 }
                    }},
                    new() { Type = FinancialCategoryType.Opex, Name = "Operasyonel Giderler", LineItems = new List<FinancialLineItem>
                    {
                        new() { Name = "Enerji maliyeti", Value = 5_000_000 }
                    }},
                    new() { Type = FinancialCategoryType.Capex, Name = "Kısa Vadeli Yatırımlar", LineItems = new List<FinancialLineItem>
                    {
                        new() { Name = "Ekipman", Value = 10_000_000 }
                    }},
                    new() { Type = FinancialCategoryType.CashFlow, Name = "Uzun Vadeli Nakit Akışı", LineItems = new List<FinancialLineItem>
                    {
                        new() { Name = "Nakit akışı", Value = 30_000_000 }
                    }}
                }
            };
        }
    }

    private string GetCategoryIcon(FinancialCategoryType type) => type switch
    {
        FinancialCategoryType.Revenue => "💰",
        FinancialCategoryType.Opex => "🔧",
        FinancialCategoryType.Capex => "🏭",
        FinancialCategoryType.CashFlow => "📊",
        _ => "📋"
    };

    private string GetCategoryLabel(FinancialCategoryType type) => type switch
    {
        FinancialCategoryType.Revenue => "Gelir",
        FinancialCategoryType.Opex => "Operasyonel Giderler",
        FinancialCategoryType.Capex => "Kısa Vadeli Yatırımlar",
        FinancialCategoryType.CashFlow => "Uzun Vadeli Nakit Akışı",
        _ => type.ToString()
    };

    private void AddLineItem(FinancialCategory category)
    {
        category.LineItems.Add(new FinancialLineItem
        {
            Name = "",
            Value = 0
        });
    }

    private async Task Close()
    {
        await IsOpenChanged.InvokeAsync(false);
    }

    private async Task Save()
    {
        await OnSave.InvokeAsync(editProfile);
        await Close();
    }
}
