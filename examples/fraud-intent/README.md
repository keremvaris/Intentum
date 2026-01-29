# Fraud / Abuse Intent Detection

This example shows Intentum for **fraud/abuse intent detection**: inferring whether observed behavior indicates suspicious access or legitimate account recovery, then feeding a policy decision (Block / Observe / Allow).

## Run

```bash
dotnet run --project examples/fraud-intent
```

No API key required; uses the Mock embedding provider.

## What it does

1. **Observed behavior** — Login failures, IP change, retries, captcha, password reset, success, device verification.
2. **Infer intent** — `LlmIntentModel` (Mock) produces confidence and signals from the behavior space.
3. **Decide** — A simple policy maps confidence and signal count to Block / Observe / Allow (in production you would call StepUpAuth(), Allow(), Monitor() instead of just printing).

## Docs

See [Real-world scenarios — Fraud / Abuse](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md#use-case-1-fraud--abuse-intent-detection) in the documentation.
