# Usage Scenarios (EN)

Intentum does not use Given/When/Then steps. Instead you **Observe** events, **Infer** intent and confidence, and **Decide** with policy rules. This page shows both **classic** (payment, login, support) and **ESG / compliance** scenarios: what behavior you record, how you define the policy, and what outcome to expect.

For the replacement of Given/When/Then, see [index](index.md#what-replaced-givenwhenthen). For types and flow, see [API Reference](api.md).

---

## Classic scenarios

### 1) Payment flow with retries

**What it is:** The user logs in, attempts payment (sometimes with retries), then submits. This is normal “complete payment” behavior — not suspicious.

**Behavior (Observe):**  
`user:login` → `user:payment_attempt` → `user:retry` → `user:payment_attempt` → `user:submit` (or similar).

**Policy idea:** If confidence is High or Certain → Allow. If Medium → Observe. You can add a separate rule to Block when retry count is too high (see “Suspicious retries” below).

**Code (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "payment_attempt")
    .Observe("user", "retry")
    .Observe("user", "payment_attempt")
    .Observe("user", "submit");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Expected outcome:** Typically **Allow** or **Observe**, depending on inferred confidence.

---

### 2) Suspicious retries (login / payment)

**What it is:** The user retries login or payment many times with no clear “submit” or success. This can indicate abuse or a stuck flow.

**Behavior (Observe):**  
`user:login` → `user:retry` → `user:retry` → `user:retry` (four events; three retries).

**Policy idea:** Block when retry count (e.g. signals containing “retry”) exceeds a threshold (e.g. ≥ 3). Add this rule **before** Allow.

**Code (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "retry")
    .Observe("user", "retry");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "ExcessiveRetryBlock",
        i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Expected outcome:** **Block** when the “excessive retry” rule matches; otherwise Allow or Observe by confidence.

---

## ESG and compliance scenarios

### 3) ESG report submission with retries

**What it is:** An analyst prepares an ESG report, retries validation a couple of times, then submits. This is normal “ESG reporting with retries” behavior — not suspicious.

**Behavior (Observe):**  
`analyst:prepare_esg_report` → `analyst:retry_validation` → `analyst:retry_validation` → `system:report_submitted` (four events).

**Policy idea:** If confidence is High or Certain → Allow. If Medium → Observe (monitor). You might also add a rule that blocks only when retry count is very high (see scenario 2).

**Code (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("analyst", "prepare_esg_report")
    .Observe("analyst", "retry_validation")
    .Observe("analyst", "retry_validation")
    .Observe("system", "report_submitted");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Expected outcome:** Typically **Allow** or **Observe**, depending on the inferred confidence. No Block unless you add a rule that fires on retry count.

---

### 4) ESG report with excessive retries

**What it is:** An analyst prepares an ESG report then retries validation many times without a clear “submit.” This can indicate compliance issues, data quality problems, or a stuck flow.

**Behavior (Observe):**  
`analyst:prepare_esg_report` → `analyst:retry_validation` → `analyst:retry_validation` → `analyst:retry_validation` (four events; three retries).

**Policy idea:** Block when retry count (e.g. signals containing “retry”) exceeds a threshold (e.g. ≥ 3). Add this rule **before** Allow so Block wins when both could match.

**Code (minimal):**

```csharp
var space = new BehaviorSpace()
    .Observe("analyst", "prepare_esg_report")
    .Observe("analyst", "retry_validation")
    .Observe("analyst", "retry_validation")
    .Observe("analyst", "retry_validation");

var intent = model.Infer(space);
var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "ExcessiveRetryBlock",
        i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe));

var decision = intent.Decide(policy);
```

**Expected outcome:** **Block** when the “excessive retry” rule matches (e.g. ≥ 3 retries). Otherwise Allow or Observe by confidence.

---

### 5) ESG compliance audit trail (mixed analyst, compliance, and system events)

**What it is:** Analyst, compliance, and system events are mixed (e.g. prepare_esg_report, compliance:review_esg, flag_discrepancy, retry_correction, approve, publish_esg). You still Observe all of them; the model infers intent from the full behavior; policy decides.

**Behavior (Observe):**  
`analyst:prepare_esg_report`, `compliance:review_esg`, `compliance:flag_discrepancy`, `analyst:retry_correction`, `compliance:approve`, `system:publish_esg` (or similar).

**Policy idea:** You might Block on compliance risk first, then Allow/Observe by confidence. **Order of rules matters:** first matching rule wins.

**Code (rule order example):**

```csharp
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("ComplianceRiskBlock", i => i.Signals.Any(s => s.Description.Contains("compliance", StringComparison.OrdinalIgnoreCase) && i.Confidence.Level == "Low"), PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe))
    .AddRule(new PolicyRule("WarnLow", i => i.Confidence.Level == "Low", PolicyDecision.Warn));
```

**Expected outcome:** Depends on your conditions. The first rule that matches the intent (and optional signal counts) wins; later rules are not evaluated.

---

## How to build scenarios with AI

The same **Observe** flow works with both **Mock** and a **real AI provider** (OpenAI, Gemini, etc.); the scenario design does not change, only the embedding source and confidence score do.

### Same scenario, two modes

| Mode | When to use | What happens |
|------|-------------|---------------|
| **Mock** | CI, unit tests, demos, no API key | Behavior keys (e.g. `user:login`) are scored by hash; deterministic, repeatable. |
| **Real AI** | Production, or when you want semantic similarity | The same keys are turned into vectors via an embedding API; semantically similar keys (e.g. `login` / `sign_in`) can get similar scores. |

Scenario code stays the same: `space.Observe(...)` → `model.Infer(space)` → `intent.Decide(policy)`. In the sample, if `OPENAI_API_KEY` is **not** set, Mock is used; if it **is** set, OpenAI is used; the same scenarios run, and the confidence level (High/Medium/Low) may differ by provider.

### Tips for writing AI-backed scenarios

1. **Use meaningful behavior keys** — Use consistent `actor:action` names (e.g. `user:login`, `analyst:prepare_esg_report`). Real embeddings produce semantic similarity from these strings; consistent naming gives more predictable confidence.
2. **Base policy on both confidence and signals** — Besides confidence level (High/Medium/Low), you can write rules on `intent.Signals` (e.g. retry count, or keywords like "compliance", "retry"); that way you combine AI inference with countable thresholds.
3. **Validate with Mock first, then try real AI** — Use Mock in CI and tests for deterministic results; run the same scenario with a real provider in production or for a “real confidence” demo. See [Setup – real provider](setup.md#using-a-real-provider-eg-openai) and [Providers](providers.md).

### Example: same behavior, Mock vs OpenAI

To run the same behavior first with Mock and then (via environment variable) with OpenAI, the scenario code does not change; only the embedding provider used at startup changes (in the sample, this is automatic with `OPENAI_API_KEY`). Example flow:

```csharp
// Same scenario — provider can be Mock or OpenAI
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "payment_attempt")
    .Observe("user", "submit");

var intent = model.Infer(space);  // Mock: deterministic score; OpenAI: semantic score
var decision = intent.Decide(policy);
```

With Mock you might get a given confidence (e.g. Medium); with OpenAI the same events might yield a different confidence (e.g. High). Policy rules stay the same, so the decision (Allow/Observe/Warn/Block) follows. To add new scenarios, extend the `Observe` chain and policy as in the classic and ESG examples above.

---

## Policy rule ordering

Intentum evaluates rules in the order you add them. **First matching rule wins.** So:

- Put **Block** (and strict rules) **first** when you want to deny certain behaviors regardless of confidence.
- Put **Allow** (e.g. high confidence) **after** Block so normal flows are allowed only when not blocked.
- Put **Observe** / **Warn** in between or after as needed.

**Example order:**

```csharp
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("ExcessiveRetryBlock", i => RetryCount(i) >= 3, PolicyDecision.Block))
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
    .AddRule(new PolicyRule("ObserveMedium", i => i.Confidence.Level == "Medium", PolicyDecision.Observe))
    .AddRule(new PolicyRule("WarnLow", i => i.Confidence.Level == "Low", PolicyDecision.Warn));
```

For a full runnable example with multiple scenarios, see the [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample).
