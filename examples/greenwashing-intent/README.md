# Greenwashing detection

This example shows Intentum for **sustainability-report greenwashing detection**: inferring whether observed behavioral signals indicate active greenwashing, strategic obfuscation, or genuine sustainability communication, then feeding a policy decision (Escalate / Warn / Observe / Allow) and solution suggestions.

## Run

```bash
dotnet run --project examples/greenwashing-intent
```

No API key required; uses a rule-based intent model (Intentum.Core only).

## What it does

1. **Behavior space** — Parses a sample sustainability report and records signals: vague claims (`language:claim.vague`), metrics without proof (`data:metrics.without.proof`), unsubstantiated comparisons (`language:comparison.unsubstantiated`).
2. **Intent inference** — `GreenwashingIntentModel` (custom `IIntentModel`) aggregates weighted signals and returns an intent name (e.g. `StrategicObfuscation`, `ActiveGreenwashing`) and confidence.
3. **Policy** — `IntentPolicyBuilder` maps intent name and confidence to Escalate / Warn / Observe / Allow.
4. **Solutions** — A small application-layer generator suggests actions (e.g. suspend claims, third-party audit, publish data) from intent, behavior space, and decision.

## Docs

- **EN:** [Greenwashing detection (how-to)](https://github.com/keremvaris/Intentum/blob/master/docs/en/greenwashing-detection-howto.md), [Real-world scenarios](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md)
- **TR:** [Greenwashing tespiti (how-to)](https://github.com/keremvaris/Intentum/blob/master/docs/tr/greenwashing-detection-howto.md), [Gerçek dünya senaryoları](https://github.com/keremvaris/Intentum/blob/master/docs/tr/real-world-scenarios.md)
