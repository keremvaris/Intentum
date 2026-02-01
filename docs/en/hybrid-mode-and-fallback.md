# Hybrid mode and rule-based fallback

**Why you're reading this page:** This page describes hybrid intent inference (rule-based + LLM/ONNX fallback): high-confidence cases stay fast, uncertain cases use the fallback model. It is the right place if you are balancing cost and confidence.

This page describes **hybrid intent inference**: combining rule-based logic with AI (LLM or local ONNX) so that high-confidence cases stay fast and cheap, while uncertain cases use a fallback model. It also summarizes best practices.

---

## What is hybrid mode?

**Hybrid mode** means using more than one intent model in a single inference path:

1. **Primary model** — Tried first (usually rule-based or a fast local model).
2. **Fallback model** — Used when the primary result is below a confidence threshold (or fails).

Benefits:

- **Cost:** High-confidence rule hits avoid LLM/API calls.
- **Latency:** Rules are fast; LLM is only used when needed.
- **Determinism:** Rule-matched intents are reproducible; fallback is used only when ambiguous.
- **Explainability:** Intent **Reasoning** indicates whether the result came from the primary or fallback.

---

## ChainedIntentModel (rule → LLM fallback)

**ChainedIntentModel** tries a primary model first; if confidence is below a threshold, it calls a secondary model.

### Typical setup: rules first, LLM fallback

```csharp
var rules = new List<Func<BehaviorSpace, RuleMatch?>>
{
    space =>
    {
        var loginFails = space.Events.Count(e => e.Action == "login.failed");
        if (loginFails >= 2)
            return new RuleMatch("SuspiciousAccess", 0.85, "login.failed>=2");
        return null;
    }
};

var primary = new RuleBasedIntentModel(rules);
var fallback = new LlmIntentModel(embeddingProvider, new SimpleAverageSimilarityEngine());
var chained = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);

var intent = chained.Infer(space);
// intent.Reasoning: "Primary: login.failed>=2" or "Fallback: LLM (primary confidence below 0.7)"
```

### Choosing the confidence threshold

- **Higher (e.g. 0.8):** More requests go to the fallback; safer for ambiguous cases, higher cost.
- **Lower (e.g. 0.5):** Fewer fallbacks; lower cost, but more chance of using a rule result when you might prefer LLM.

Tune using A/B tests or evaluation data (see [IntentExperiment](api.md) and [examples/ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent)).

---

## Rule → local ONNX fallback

For low latency and no external API, use a local classifier as fallback:

```csharp
var primary = new RuleBasedIntentModel(rules);
var onnxOptions = new OnnxIntentModelOptions(
    ModelPath: "path/to/intent_classifier.onnx",
    IntentLabels: ["IntentA", "IntentB", "Unknown"]);
using var fallback = new OnnxIntentModel(onnxOptions);
var chained = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);
```

See [Intentum.AI.ONNX](https://www.nuget.org/packages/Intentum.AI.ONNX) for model format (input/output shapes and intent labels).

---

## MultiStageIntentModel

When you need full control over the pipeline (signal → vector → intent → confidence), use **MultiStageIntentModel**. It is useful for custom stages (e.g. custom vectorizer or confidence calculator). For “rule first, then fallback” the **ChainedIntentModel** is simpler and recommended.

---

## Best practices

| Goal | Recommendation |
|------|----------------|
| **Cost** | Use **ChainedIntentModel** with rules first; set threshold so most traffic stays on rules. |
| **Latency** | Prefer rule-based primary; use ONNX fallback instead of LLM when possible. |
| **Explainability** | Always use **Reasoning** (Intentum sets it to "Primary: …" or "Fallback: …"); log it for audits. |
| **Failure handling** | Wrap `model.Infer(space)` in try/catch; on API failure return a fallback intent or use a cached result. See [Production readiness](production-readiness.md) and [Embedding API errors](embedding-api-errors.md). |
| **Testing** | Unit-test rules in isolation; test ChainedIntentModel with a mock fallback to assert threshold behavior. See [examples/chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent) and [examples/ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent). |

---

## Related

- [Advanced Features](advanced-features.md) — RuleBasedIntentModel, ChainedIntentModel, fluent API
- [Real-world scenarios](real-world-scenarios.md) — Chained intent (rule → LLM fallback)
- [Production readiness](production-readiness.md) — Fallback and error handling
- [API overview](api.md) — ChainedIntentModel, OnnxIntentModel, Intent.Reasoning

**Next step:** When you're done with this page → [Real-world scenarios](real-world-scenarios.md) or [Production readiness](production-readiness.md).
