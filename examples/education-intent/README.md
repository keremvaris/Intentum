# Education Risk Intent Detection

Detects student risk: dropout probability, academic integrity concerns.

## Run

```bash
dotnet run --project examples/education-intent
```

No API key required; uses the Mock embedding provider.

## What it does

1. Observes student behavior events (attendance, grades, submissions).
2. Infers intent using LlmIntentModel with Mock provider.
3. Applies policy: Escalate (critical), Warn (at-risk), Observe (watch), Allow (normal).