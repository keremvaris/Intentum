# API Reference (EN)

This page explains the main types and how they fit together. For full method signatures and generated docs, see the [API site](https://keremvaris.github.io/Intentum/api/).

---

## How Intentum works (typical flow)

1. **Observe** — You record user or system events (e.g. login, retry, submit) into a **BehaviorSpace**.
2. **Infer** — An **LlmIntentModel** (with an embedding provider and similarity engine) turns that behavior into an **Intent** and a confidence level (High / Medium / Low / Certain).
3. **Decide** — You pass the **Intent** and an **IntentPolicy** (rules) to **Decide**; you get **Allow**, **Observe**, **Warn**, or **Block**.

So: *behavior → intent → policy decision*. No hard-coded scenario steps; the model infers intent from the observed events.

---

## Core (`Intentum.Core`)

| Type | What it does |
|------|----------------|
| **BehaviorSpace** | Container for observed events. You call `.Observe(actor, action)` (e.g. `"user"`, `"login"`). Use `.ToVector()` to get a behavior vector for inference. |
| **Intent** | Result of inference: confidence level, score, and signals (contributing behaviors with weights). |
| **IntentConfidence** | Part of Intent: `Level` (string) and `Score` (0–1). |
| **IntentEvaluator** | Evaluates intent against criteria; used internally by the model. |

**Where to start:** Create a `BehaviorSpace`, call `.Observe(...)` for each event, then pass the space to your intent model’s `Infer(space)`.

---

## Runtime (`Intentum.Runtime`)

| Type | What it does |
|------|----------------|
| **IntentPolicy** | Ordered list of rules. Add rules with `.AddRule(PolicyRule(...))`. First matching rule wins. |
| **PolicyRule** | Name + condition (e.g. lambda on `Intent`) + **PolicyDecision** (Allow, Observe, Warn, Block). |
| **IntentPolicyEngine** | Evaluates an intent against a policy; returns a **PolicyDecision**. |
| **RuntimeExtensions.Decide** | Extension: `intent.Decide(policy)` — runs the policy and returns the decision. |
| **RuntimeExtensions.ToLocalizedString** | Extension: `decision.ToLocalizedString(localizer)` — human-readable text (e.g. for UI). |
| **IIntentumLocalizer** / **DefaultLocalizer** | Localization for decision labels (e.g. "Allow", "Block"). **DefaultLocalizer** uses a culture (e.g. `"tr"`). |

**Where to start:** Build an `IntentPolicy` with `.AddRule(...)` in the order you want (e.g. Block rules first, then Allow). Call `intent.Decide(policy)` after inference.

---

## AI (`Intentum.AI`)

| Type | What it does |
|------|----------------|
| **IIntentEmbeddingProvider** | Turns a behavior key (e.g. `"user:login"`) into an **IntentEmbedding** (vector + score). Implemented by each provider (OpenAI, Gemini, etc.) or **MockEmbeddingProvider** for tests. |
| **IIntentSimilarityEngine** | Combines embeddings into a single similarity score. **SimpleAverageSimilarityEngine** is the built-in option. |
| **LlmIntentModel** | Takes an embedding provider + similarity engine; **Infer(BehaviorSpace)** returns an **Intent** with confidence and signals. |

**Where to start:** Use **MockEmbeddingProvider** and **SimpleAverageSimilarityEngine** for a quick local run; swap in a real provider (see [Providers](providers.md)) for production.

**AI pipeline (summary):**  
1) **Embedding** — Each behavior key (`actor:action`) is sent to the provider; returns vector + score. Mock = hash; real provider = semantic embedding.  
2) **Similarity** — All embeddings are combined into a single score (e.g. average).  
3) **Confidence** — The score is mapped to High/Medium/Low/Certain.  
4) **Signals** — Each behavior’s weight appears in Intent signals; usable in policy rules (e.g. retry count).

---

## Providers (optional packages)

| Type | What it does |
|------|----------------|
| **OpenAIEmbeddingProvider** | Uses OpenAI embedding API; configure with **OpenAIOptions** (e.g. `FromEnvironment()`). |
| **GeminiEmbeddingProvider** | Uses Google Gemini embedding API; **GeminiOptions**. |
| **MistralEmbeddingProvider** | Uses Mistral embedding API; **MistralOptions**. |
| **AzureOpenAIEmbeddingProvider** | Uses Azure OpenAI embedding deployment; **AzureOpenAIOptions**. |
| **ClaudeMessageIntentModel** | Claude-based intent model (message scoring); **ClaudeOptions**. |

Register providers via the **AddIntentum\*** extension methods and options (env vars). See [Providers](providers.md) for setup and env vars.

---

## Minimal code reference

```csharp
// 1) Build behavior
var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

// 2) Infer intent (Mock = no API key)
var model = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());
var intent = model.Infer(space);

// 3) Decide
var policy = new IntentPolicy()
    .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow));
var decision = intent.Decide(policy);
```

For a full runnable example, see the [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) and [Setup](setup.md).
