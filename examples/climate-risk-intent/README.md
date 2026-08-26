# Climate Risk Assessment

This example shows Intentum for **climate risk assessment** using SSP, RCP, and WRI scenarios: inferring physical, transition, and economic risks across sectors and time horizons.

## Run

```bash
# Console mode
dotnet run --project examples/climate-risk-intent

# Web UI mode
dotnet run --project examples/climate-risk-intent -- --web

# Custom port
dotnet run --project examples/climate-risk-intent -- --web --port=8080
```

No API key required; uses a rule-based intent model (Intentum.Core only).

## What it does

1. **Scenarios** — SSP1-2.6 to SSP5-8.5, RCP2.6 to RCP8.5, WRI water stress and energy transition scenarios.
2. **Behavior Space** — Generates behavioral signals based on scenario, sector, and time horizon.
3. **Intent Inference** — `ClimateRiskIntentModel` aggregates weighted signals and returns intent (CriticalClimateRisk to MinimalClimateRisk) and confidence.
4. **Policy** — `ClimateRiskPolicy` maps intent to Escalate/Warn/Observe/Allow decisions.
5. **Risk Calculation** — Physical risk (flood, drought, storm, sea level, heatwave) and transition risk (policy, technology, market, reputation) calculators.
6. **Economic Impact** — GDP, investment, insurance, borrowing, and workforce impact analysis.
7. **Web UI** — Interactive dashboard with scenario selection, risk heat map, and recommended actions.

## Sectors

- Energy, Agriculture, Real Estate, Finance, Tourism
- Each sector has unique physical and transition sensitivity profiles

## Documentation

- **EN:** [Real-world scenarios](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md)
- **TR:** [Gerçek dünya senaryoları](https://github.com/keremvaris/Intentum/blob/master/docs/tr/real-world-scenarios.md)
