# Chained Intent (Rule → LLM Fallback)

This example shows **ChainedIntentModel**: try a rule-based (or keyword) model first; if confidence is below a threshold, fall back to an LLM model. This reduces cost and latency while keeping high-confidence rule hits deterministic and explainable.

## Run

```bash
dotnet run --project examples/chained-intent
```

No API key required (fallback uses Mock embedding provider).

## What it does

1. **Primary model** — `RuleBasedIntentModel` with rules such as:
   - `login.failed >= 2` and `password.reset` and `login.success` → **AccountRecovery** (0.85)
   - `login.failed >= 3` and `ip.changed` → **SuspiciousAccess** (0.9)
2. **Fallback model** — `LlmIntentModel` (Mock or real provider) when no rule matches or primary confidence &lt; threshold.
3. **Chain** — `ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7)`.
4. **Reasoning** — Each intent includes `Reasoning` (e.g. "Primary: login.failed>=2 and password.reset and login.success" or "Fallback: LLM (primary confidence below 0.7)").

## Docs

- [Real-world scenarios — Chained intent](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md#chained-intent-rule--llm-fallback)
- [Advanced features — Rule-based and chained models](https://github.com/keremvaris/Intentum/blob/master/docs/en/advanced-features.md)
