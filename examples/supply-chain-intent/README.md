# Supply Chain Risk Intent Detection

Detects supply chain risks: stock depletion, supplier reliability issues, logistics disruptions.

## Run

```bash
dotnet run --project examples/supply-chain-intent
```

No API key required; uses the Mock embedding provider.

## What it does

1. Observes supply chain behavior events (inventory, supplier, logistics).
2. Infers intent using LlmIntentModel with Mock provider.
3. Applies policy: Block (critical), Warn (high), Observe (medium), Allow (low).
