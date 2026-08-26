# Climate Risk - Company Financial Profiles

## Overview

This module extends the Intentum climate risk assessment with company-specific financial impact analysis. It translates physical and transition climate risks into monetary terms, enabling business decision-makers to understand the financial exposure of their operations.

## Architecture

### Data Models (`CompanyProfile.cs`)

- **CompanyProfile** — Company identity, sector, location, and financial categories
- **FinancialCategory** — Groups line items by type (Revenue, Opex, Capex, Cash Flow)
- **FinancialLineItem** — Individual financial entries with name and annual value (TRY)
- **FinancialImpact** — Calculated impact per category and overall net cash flow

### Services

- **CompanyProfileService** — CRUD operations with 3 seed profiles (Manufacturing/Ankara, Energy/İzmir, Tourism/Antalya)
- **FinancialImpactEngine** — Translates risk scores into monetary impact per line item
- **ScenarioComparisonEngine** — Runs all 4 SSP scenarios in parallel for comparison

### Integration

- **RiskCalculationEngine** — Optionally calculates financial impact when `CompanyProfileId` is provided
- **ClimateRiskIntentModel** — Includes economic signal weights for financial-aware decisions
- **Blazor Dashboard** — Profile selector, scenario comparison grid, financial impact cards, drawer editor

## Signal Weights

| Signal | Weight | Source |
|--------|--------|--------|
| `economic:cost_of_goods` | 0.9 | COGS risk exposure |
| `economic:operational_expenses` | 0.85 | OPEX vulnerability |
| `economic:revenue_at_risk` | 0.95 | Revenue at risk from climate events |
| `economic:capital_expenditure` | 0.8 | CAPEX disruption risk |

## Financial Impact Calculation

For each financial line item:

```
PhysicalImpact = LineItemValue × PhysicalRiskScore × SignalBoost
TransitionImpact = LineItemValue × TransitionRiskScore × SignalBoost
```

Net Cash Flow Impact = Σ(RevenueImpact) - Σ(OpexImpact) - Σ(CapexImpact) + Σ(CashFlowImpact)

Negative net = financial loss; Positive net = financial gain from climate factors.

## Seed Profiles

| Company | Sector | Location | Revenue | OPEX | CAPEX |
|---------|--------|----------|---------|------|-------|
| Ankara Sanayi A.Ş. | Sanayi | Ankara | 85M ₺ | 35M ₺ | 22M ₺ |
| İzmir Enerji Ltd. | Enerji | İzmir | 120M ₺ | 45M ₺ | 40M ₺ |
| Antalya Turizm A.Ş. | Turizm | Antalya | 65M ₺ | 28M ₺ | 8M ₺ |

## Testing

- `FinancialImpactEngineTests` — 5 tests validating monetary calculations
- `ScenarioComparisonEngineTests` — 1 test validating parallel SSP comparison

Run tests:
```bash
dotnet test tests/Intentum.Sample.Blazor.Tests/
```
