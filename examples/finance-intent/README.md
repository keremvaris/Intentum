# Finance Risk Intent Detection

Detects financial risks: credit risk, market volatility, portfolio exposure.

## Run

```bash
dotnet run --project examples/finance-intent
```

No API key required; uses the Mock embedding provider.

## What it does

1. Observes financial behavior events (payments, balance, market data).
2. Infers intent using LlmIntentModel with Mock provider.
3. Applies policy: Block (high risk), Warn (elevated), Observe (monitor).