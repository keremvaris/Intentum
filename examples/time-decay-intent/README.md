# Time Decay Intent

This example shows **TimeDecaySimilarityEngine** with **LlmIntentModel**: more recent events have higher influence on intent inference (exponential decay by age). Useful for fraud/risk where "login fail 5 min ago" should weigh more than "login fail 1 hour ago".

## Run

```bash
dotnet run --project examples/time-decay-intent
```

No API key required (Mock embedding provider).

## What it does

1. **TimeDecaySimilarityEngine** — half-life 1 hour; events older than 1h have half the weight.
2. **LlmIntentModel** — when the engine implements `ITimeAwareSimilarityEngine`, the model calls `CalculateIntentScoreWithTimeDecay(behaviorSpace, embeddings)` automatically (no extra wiring).
3. **Behavior events with timestamps** — `BehaviorEvent(actor, action, occurredAt)` so decay can be applied per event.

## Docs

- [Advanced features — TimeDecaySimilarityEngine](https://github.com/keremvaris/Intentum/blob/master/docs/en/advanced-features.md#timedecaysimilarityengine)
- [API — ITimeAwareSimilarityEngine](https://github.com/keremvaris/Intentum/blob/master/docs/en/api.md)
