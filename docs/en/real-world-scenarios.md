# Real-world scenarios

Intentum in production: fraud/abuse detection and AI decision fallback.

---

## Use case 1: Fraud / Abuse intent detection

### The problem (classic approach)

Classic systems do:

```
IF too_many_failures AND unusual_ip AND velocity_high
THEN block_user
```

Issues:

- False positives
- Rules break when flows change
- AI signals do not fit scenarios
- "Suspicious but innocent" cases explode

### The question Intentum asks

Is this user actually trying to commit fraud, or just having a failed login experience?

That is not a binary question.

### 1. Observed behavior space

```csharp
var space = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "ip.changed")
    .Observe("user", "login.retry")
    .Observe("user", "captcha.passed");
```

Note: order is not critical; repetition carries information; "success" events are also signals.

### 2. Intent inference

```csharp
var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = intentModel.Infer(space);
```

The model treats:

- "failed login" as a negative signal
- "captcha passed" as a positive signal
- "ip change" as risk-increasing but not proof by itself

### 3. Assert on risk intent

With a custom intent model that returns named intents (e.g. SuspiciousAccess, AccountRecovery):

```csharp
// intent.Name == "SuspiciousAccess"
// intent.Confidence.Score in range 0.4–0.7
```

This says: "This user is likely risky, but not definitely fraud."

In BDD the test would pass or fail. In Intentum the test *informs* the decision.

### 4. Alternative behavior, same intent

```csharp
var space2 = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "password.reset")
    .Observe("user", "login.success")
    .Observe("user", "device.verified");
```

Result (with an appropriate model): intent name "AccountRecovery", confidence ~0.8.

Flow changed. Intent changed. The system did not break.

### 5. Decision layer (critical)

Intentum does not block. It feeds the decision.

```csharp
if (intent.Name == "SuspiciousAccess" && intent.Confidence.Score > 0.65)
{
    StepUpAuth();
}
else if (intent.Confidence.Score < 0.4)
{
    Allow();
}
else
{
    Monitor();
}
```

Intent correctness ≠ action correctness. You keep that separation explicit.

### 6. Why this is hard with BDD

In BDD you would need scenarios for:

- login failed twice from new IP
- login failed three times with captcha
- password reset from new device
- login retry after reset
- captcha success after IP change
- …

Combinatorial explosion. Unmaintainable. AI signals do not fit into fixed scenarios.

With Intentum: one model, a broad behavior space, and confidence-driven decisions.

### 7. Takeaway

This example shows that Intentum is not a test framework; it is a *meaning layer* before the decision.

---

## Use case 2: AI decision fallback & validation

When the model is confident… but wrong.

### The problem (real world)

An LLM/ML model:

- sometimes misunderstands
- sometimes hallucinates
- sometimes is overconfident

Classic systems: "Model gave an answer → accept it."

With BDD you test: fixed prompt, expected string, any deviation = fail. That does not match AI reality.

### The question Intentum asks

What was the model *trying* to do when it made this decision? Is this behavior consistent with the intended intent?

That question is model-agnostic. GPT, Claude, local LLM — same idea.

### Scenario: AI-assisted decision engine

Example: an AI classifies user requests into:

- Refund
- Technical support
- Account issue
- Abuse attempt

### 1. Observed AI behavior space

Here "behavior" is the *model's* behavior, not the user's.

```csharp
var space = new BehaviorSpace()
    .Observe("llm", "high_confidence")
    .Observe("llm", "short_reasoning")
    .Observe("llm", "no_followup_question")
    .Observe("user", "rephrased_request")
    .Observe("llm", "changed_answer");
```

These signals say: the model decided quickly, gave a weak explanation, the user was not satisfied, the model backtracked.

### 2. Infer AI intent

```csharp
var intent = intentModel.Infer(space);
```

Result (with a suitable model): intent name e.g. "PrematureClassification", confidence 0.78.

Meaning: "The model was trying to answer quickly, not to classify carefully."

BDD cannot capture this.

### 3. Intent-based fallback

```csharp
if (intent.Name == "PrematureClassification" && intent.Confidence.Score > 0.7)
{
    RouteToHuman();
    ReduceModelTrust();
}
```

Important: you are not saying the model is "wrong"; you are acting on its *intent*. You adjust trust. That is production-grade AI behavior.

### 4. Alternative behavior, healthy intent

```csharp
var space2 = new BehaviorSpace()
    .Observe("llm", "asked_clarifying_question")
    .Observe("user", "provided_details")
    .Observe("llm", "reasoning_explicit")
    .Observe("llm", "moderate_confidence");
```

Result: intent "CarefulUnderstanding", confidence ~0.85.

Decision: `AllowAutoDecision();`

### 5. Critical difference

BDD / unit tests ask: "Is the output correct?"

Intentum asks: "How and why was this output produced?"

In AI systems, *why* is often more valuable than the raw output.

### 6. What Intentum enables here

- Model version change
- Prompt change
- Vendor change
- Temperature / sampling change

None of these need to break tests, as long as intent stays aligned.

### 7. Message for AI engineers

"We solved this in production the hard way." — Intentum is for that gap.

### Intentum's real position

Intentum is:

- **not** a test framework
- **not** a prompt tool
- **not** an AI wrapper

It is:

- an **AI reasoning validation layer**
- a **decision confidence engine**
- an **intent-aware safety net**

---

## Use case 3: Chained intent (Rule → LLM fallback)

Use rules first; call the LLM only when no rule matches or confidence is below a threshold. Reduces cost and latency; keeps high-confidence rule hits deterministic and explainable.

### The idea

1. **Primary model** — RuleBasedIntentModel: e.g. "login.failed >= 2 and password.reset and login.success" → AccountRecovery (0.85).
2. **Fallback model** — LlmIntentModel when no rule matches or primary confidence &lt; threshold.
3. **ChainedIntentModel** — `ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7)`.

### Code

```csharp
var rules = new List<Func<BehaviorSpace, RuleMatch?>>
{
    space =>
    {
        var loginFails = space.Events.Count(e => e.Action == "login.failed");
        var hasReset = space.Events.Any(e => e.Action == "password.reset");
        var hasSuccess = space.Events.Any(e => e.Action == "login.success");
        if (loginFails >= 2 && hasReset && hasSuccess)
            return new RuleMatch("AccountRecovery", 0.85, "login.failed>=2 and password.reset and login.success");
        return null;
    }
};

var primary = new RuleBasedIntentModel(rules);
var fallback = new LlmIntentModel(embeddingProvider, new SimpleAverageSimilarityEngine());
var chained = new ChainedIntentModel(primary, fallback, confidenceThreshold: 0.7);

var intent = chained.Infer(space);
// intent.Reasoning: "Primary: ..." or "Fallback: LLM (primary confidence below 0.7)"
```

### Explainability

Each intent includes **Reasoning** (e.g. which rule matched, or that the fallback was used). Use it for logging, debugging, and "why did we get this decision?"

See [examples/chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent) for a runnable example.

---

## Runnable examples

Minimal runnable projects for these scenarios:

- [examples/fraud-intent](https://github.com/keremvaris/Intentum/tree/master/examples/fraud-intent) — Fraud / abuse intent, policy → StepUpAuth / Allow / Monitor
- [examples/customer-intent](https://github.com/keremvaris/Intentum/tree/master/examples/customer-intent) — Customer intent (purchase, support), policy → Allow / Observe / route by intent
- [examples/greenwashing-intent](https://github.com/keremvaris/Intentum/tree/master/examples/greenwashing-intent) — Greenwashing detection from report text
- [examples/ai-fallback-intent](https://github.com/keremvaris/Intentum/tree/master/examples/ai-fallback-intent) — AI decision fallback (PrematureClassification / CarefulUnderstanding)
- [examples/chained-intent](https://github.com/keremvaris/Intentum/tree/master/examples/chained-intent) — Rule → LLM fallback
- [examples/time-decay-intent](https://github.com/keremvaris/Intentum/tree/master/examples/time-decay-intent) — Recent events weigh more
- [examples/vector-normalization](https://github.com/keremvaris/Intentum/tree/master/examples/vector-normalization) — Cap, L1, SoftCap

**Web sample (fraud + greenwashing + explain):** [samples/Intentum.Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) — full UI and HTTP API: intent infer/explain, greenwashing detection (multi-language, optional image, Scope 3/blockchain mock), Dashboard with analytics and recent analyses. See [Greenwashing detection (how-to)](greenwashing-detection-howto.md#6-sample-application-intentumsampleweb) and [Setup – Web sample](setup.md#build-and-run-the-repo-samples).

See the [examples README](https://github.com/keremvaris/Intentum/tree/master/examples) in the repo for how to run the console examples.

For more flows (payment, support, ESG), see [Scenarios](scenarios.md) and [Audience & use cases](audience.md).
