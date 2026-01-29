# Designing Intent Models

How meaning emerges from behavior.

---

## 1. What is intent?

Intent is:

- **not** an event
- **not** a state
- **not** a rule

Intent is the *direction* behind observed behavior.

- **Events** → happened
- **Behavior** → observed
- **Intent** → inferred

Intent is computed, not defined.

---

## 2. Intent ≠ Label

This is critical.

**Wrong:**

Intent = `"Fraud"`

**Right:**

Intent = probability distribution over hypotheses

Example:

- AccountRecovery: 0.62
- SuspiciousAccess: 0.28
- Unknown: 0.10

Intentum's **Confidence** concept comes from this.

---

## 3. How intent models work

Intentum does not impose a single method.

There are three levels:

### Level 1 — Heuristic intent models

Simplest and fastest.

```csharp
if (space.Events.Count(e => e.Action == "login.failed") > 2 &&
    space.Events.Any(e => e.Action == "password.reset"))
{
    return new Intent("AccountRecovery", signals, IntentConfidence.FromScore(0.8));
}
```

**Pros:** deterministic, explainable, fast  
**Cons:** does not scale, limited pattern variety

### Level 2 — Weighted signal models

This is where real-world use begins.

- `login.failed` → +0.3 risk
- `captcha.passed` → -0.2 risk
- `device.verified` → -0.4 risk

Confidence: `confidence = Σ(weights × signals)`

**Pros:** flexible, tolerant, tunable  
Intentum's default model is close to this.

### Level 3 — AI-assisted intent models

Here AI enters the loop.

- embeddings
- clustering
- LLM reasoning
- hybrid scoring

But:

**AI does not produce intent.**  
**AI produces intent *signals*.**

The final decision stays in Intentum.

This is a deliberate design.

---

## 4. What confidence is (and is not)

Confidence is:

- **not** accuracy
- **not** truth
- **not** correctness

Confidence is:

*"How much do we trust this intent hypothesis?"*

So:

- 0.6 can be enough
- 0.9 can be suspicious

In Intentum:

- High confidence + wrong intent = 🚨
- Low confidence + correct intent = 🟢

---

## 5. Intent anti-patterns

**❌ Confusing intent with outcome**

Intent = `"Blocked"` — wrong. Intent is *before* the decision.

**❌ Tying intent to a single event**

`if (login.failed) → Fraud` — that is a reflex, not intent.

**❌ Using confidence like a boolean**

`if (intent.Confidence.Score == 1.0)` — in the AI era, this line is a red flag.

---

## 6. When is intent "correct enough"?

In Intentum, the question is:

*"Is the decision we make from this intent acceptable for the system?"*

That depends on:

- risk tolerance
- business context
- user
- time

So intent is **contextual**.

---

## 7. Intent model lifecycle

```
Observe → Infer → Decide → Learn → Adjust
```

Intentum leaves this cycle open on purpose:

- learning can live outside
- AI can be added later
- human feedback can be included

---

## 8. Intentum's philosophical boundary

Intentum does **not**:

- claim that intent is "true"
- simulate the human mind
- promise absolute correctness

Intentum **does**:

Make reasonable decisions under uncertainty possible.
