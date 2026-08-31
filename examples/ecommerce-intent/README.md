# E-commerce Risk Intent Detection

Detects e-commerce risks: fake orders, cart abandonment, customer churn.

## Run

```bash
dotnet run --project examples/ecommerce-intent
```

No API key required; uses the Mock embedding provider.

## What it does

1. Observes e-commerce behavior events (account, order, payment, browsing).
2. Infers intent using LlmIntentModel with Mock provider.
3. Applies policy: Block (fraudulent), Warn (suspicious), Observe (watch).
