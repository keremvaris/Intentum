# Architecture (EN)

**Why you're reading this page** — This page explains Intentum’s architecture: which packages to use and why, and how the Observe → Infer → Decide flow maps to the code. Useful before setting up your first project or when choosing packages.

---

## Core flow: Observe → Infer → Decide

Intentum replaces scenario-based BDD with a three-step flow: record behavior, infer intent, then apply policy.

```mermaid
flowchart LR
  subgraph observe [Observe]
    Events[Behavior Events]
    Space[BehaviorSpace]
    Events --> Space
  end
  subgraph infer [Infer]
    Model[IIntentModel]
    Intent[Intent]
    Space --> Model
    Model --> Intent
  end
  subgraph decide [Decide]
    Policy[IntentPolicy]
    Decision[PolicyDecision]
    Intent --> Policy
    Policy --> Decision
  end
  observe --> infer --> decide
```

| Step | Responsibility |
|------|----------------|
| **Observe** | Record what happened: `space.Observe(actor, action)` or `BehaviorSpaceBuilder`. Events form a **BehaviorSpace**. Use **ToVector(options)** for optional normalization (Cap, L1, SoftCap). |
| **Infer** | **IIntentModel** (e.g. **LlmIntentModel**, **RuleBasedIntentModel**, or **ChainedIntentModel**) produces **Intent** (name, confidence, signals, optional **Reasoning**). LlmIntentModel uses embeddings + similarity engine; dimension counts as weights; **ITimeAwareSimilarityEngine** (e.g. TimeDecay) applied automatically when used. |
| **Decide** | **IntentPolicy** evaluates rules in order → **PolicyDecision** (Allow, Observe, Warn, Block, Escalate, RequireAuth, RateLimit). |

**Concrete example (e-commerce account takeover):**

- **Observe:** Alex, within one minute: 1) added 5 different credit cards, 2) placed orders to 10 different addresses, 3) changed the account password. The system records these events in a **BehaviorSpace**.
- **Infer:** **IIntentModel** infers intent from this behavior: Intent = AccountTakeover, confidence 92%. The system interprets this behavior as an "account takeover" intent.
- **Decide:** **IntentPolicy** rule: "above 85% confidence → Block + alert". Result: order is blocked, alert is sent to the security team.

---

## Package architecture

Packages are grouped by responsibility: core types, runtime (policy + rate limiting), AI (model + embeddings + caching), persistence, analytics, and optional extensions.

```mermaid
flowchart TB
  subgraph core [Core]
    CorePkg[Intentum.Core]
    CorePkg --> Behavior[BehaviorSpace, BehaviorEvent, BehaviorSpaceBuilder, ToVectorOptions]
    CorePkg --> Intent["Intent, IntentConfidence, IntentSignal, Reasoning (Intentum.Core.Intents)"]
    CorePkg --> Batch[IBatchIntentModel, BatchIntentModel]
    CorePkg --> Model[IIntentModel]
    CorePkg --> RuleBased[RuleBasedIntentModel, ChainedIntentModel]
  end

  subgraph runtime [Runtime]
    RuntimePkg[Intentum.Runtime]
    RuntimePkg --> Policy[IntentPolicy, IntentPolicyBuilder, PolicyDecision]
    RuntimePkg --> RateLimit[IRateLimiter, MemoryRateLimiter]
    RuntimePkg --> Localization[IIntentumLocalizer]
  end

  subgraph ai [AI]
    AIPkg[Intentum.AI]
    AIPkg --> Embeddings[IIntentEmbeddingProvider, LlmIntentModel]
    AIPkg --> Similarity[IIntentSimilarityEngine, ITimeAwareSimilarityEngine, SimpleAverage, TimeDecay, Cosine, Composite]
    AIPkg --> Cache[IEmbeddingCache, MemoryEmbeddingCache, CachedEmbeddingProvider]
  end

  subgraph providers [AI Providers]
    OpenAI[Intentum.AI.OpenAI]
    Gemini[Intentum.AI.Gemini]
    Mistral[Intentum.AI.Mistral]
    Azure[Intentum.AI.AzureOpenAI]
    Claude[Intentum.AI.Claude]
  end

  subgraph persistence [Persistence]
    Persist[Intentum.Persistence]
    PersistEF[Intentum.Persistence.EntityFramework]
    PersistMongo[Intentum.Persistence.MongoDB]
    PersistRedis[Intentum.Persistence.Redis]
    Persist --> Repos[IBehaviorSpaceRepository, IIntentHistoryRepository]
    PersistEF --> Repos
    PersistMongo --> Repos
    PersistRedis --> Repos
  end

  subgraph extensions [Extensions]
    AspNet[Intentum.AspNetCore]
    Analytics[Intentum.Analytics]
    Clustering[Intentum.Clustering]
    Events[Intentum.Events]
    Experiments[Intentum.Experiments]
    Explainability[Intentum.Explainability]
    Simulation[Intentum.Simulation]
    MultiTenancy[Intentum.MultiTenancy]
    Versioning[Intentum.Versioning]
    Redis[Intentum.AI.Caching.Redis]
  end

  core --> runtime
  core --> ai
  ai --> providers
  core --> persistence
  persistence --> Analytics
  core --> AspNet
  core --> Clustering
  core --> Events
  core --> Experiments
  core --> Explainability
  core --> Simulation
  core --> MultiTenancy
  core --> Versioning
  ai --> Redis
```

**All packages (summary):** Core, Runtime, AI, AI providers (OpenAI, Gemini, Mistral, Azure, Claude), Persistence (abstractions + EF, MongoDB, Redis), Analytics, AspNetCore, Clustering, Events, Experiments, Explainability, Simulation, MultiTenancy, Versioning, AI.Caching.Redis. Also Testing, Observability, Logging, CodeGen — see [API Reference](api.md) and [Advanced Features](advanced-features.md).

**Which package for which need?**

| What you need / What you want to do | Packages to get started | You can add later |
|-------------------------------------|--------------------------|-------------------|
| Intent detection only (rule-based or simple AI) | Intentum.Core | — |
| Decision rules (policy) and rate limiting | Intentum.Core + Intentum.Runtime | — |
| Intent inference with LLM (OpenAI, Gemini, etc.) | Intentum.Core + Intentum.AI + Intentum.AI.OpenAI (or other provider) | Intentum.AI.Caching.Redis (performance) |
| Store data in a database | Intentum.Core + Intentum.Persistence.EntityFramework (or MongoDB) | Intentum.Analytics |
| Use in a web app (ASP.NET Core) | Above + Intentum.AspNetCore | Intentum.MultiTenancy (multi-tenant) |

For detailed setup, see [Setup](setup.md).

---

## Inference pipeline (detail)

From a **BehaviorSpace** to a **PolicyDecision**, data flows through embedding, similarity, and policy evaluation.

```mermaid
sequenceDiagram
  participant App as Application
  participant Space as BehaviorSpace
  participant Model as IIntentModel
  participant Embed as IIntentEmbeddingProvider
  participant Cache as IEmbeddingCache
  participant Sim as IIntentSimilarityEngine
  participant Policy as IntentPolicy
  participant Rate as IRateLimiter

  App->>Space: Observe(actor, action)
  App->>Model: Infer(space)
  Model->>Space: Events / ToVector (optional ToVectorOptions)
  Model->>Embed: Embed(behaviorKey)
  Embed->>Cache: Get(key)
  alt cache miss
    Embed->>Embed: Call underlying provider
    Embed->>Cache: Set(key, embedding)
  end
  Embed-->>Model: IntentEmbedding
  Model->>Sim: CalculateIntentScore(embeddings, optional sourceWeights) or CalculateIntentScoreWithTimeDecay(space, embeddings) if ITimeAwareSimilarityEngine
  Sim-->>Model: score
  Model-->>App: Intent (name, confidence, signals, optional Reasoning)
  App->>Policy: intent.Decide(policy)
  Policy-->>App: PolicyDecision
  opt when decision is RateLimit
    App->>Rate: TryAcquireAsync(key, limit, window)
    Rate-->>App: RateLimitResult
  end
```

---

## Layer view

A simplified view of how layers depend on each other (no package names in nodes to keep the diagram readable).

```mermaid
flowchart TB
  subgraph app [Application / Samples]
    WebApp[Sample.Blazor]
    ConsoleApp[Sample.Console]
  end

  subgraph host [Hosting / HTTP]
    AspNetCore[Middleware, HealthChecks]
    Events[Webhooks, IIntentEventHandler]
  end

  subgraph domain [Domain]
    Behavior[Behavior Space]
    Intent[Intent]
    Policy[Policy]
  end

  subgraph ai_layer [AI Layer]
    Embedding[Embedding Provider]
    Similarity[Similarity Engine]
    Cache[Embedding Cache]
    Model[Intent Model]
  end

  subgraph data [Data]
    Repo[Repositories]
    Analytics[Analytics]
    Clustering[Clustering]
    Events[Events/Webhook]
    Experiments[Experiments]
  end

  app --> host
  app --> domain
  app --> ai_layer
  app --> data
  host --> domain
  ai_layer --> domain
  data --> domain
```

---

## Optional flows

### Persistence and analytics

Intent history is stored via **IIntentHistoryRepository**; **IIntentAnalytics** consumes it for trends, decision distribution, anomaly detection, and export.

```mermaid
flowchart LR
  Infer[Infer + Decide]
  Infer --> Save[SaveAsync]
  Save --> History[(Intent History)]
  History --> Analytics[IIntentAnalytics]
  Analytics --> Trends[Trends]
  Analytics --> Distribution[Decision Distribution]
  Analytics --> Anomaly[Anomaly Detection]
  Analytics --> Export[JSON / CSV Export]
```

### Rate limiting

When the policy returns **RateLimit**, the application checks **IRateLimiter** (e.g. **MemoryRateLimiter**) and can return 429 with Retry-After.

```mermaid
flowchart LR
  Intent[Intent]
  Policy[IntentPolicy]
  Intent --> Policy
  Policy --> Decision{Decision}
  Decision -->|RateLimit| Limiter[IRateLimiter.TryAcquireAsync]
  Decision -->|Other| Done[Return decision]
  Limiter --> Allowed{Allowed?}
  Allowed -->|Yes| Done
  Allowed -->|No| RetryAfter[429 Retry-After]
```

### Multi-tenancy

**TenantAwareBehaviorSpaceRepository** wraps **IBehaviorSpaceRepository** and **ITenantProvider**: it injects `TenantId` into metadata on save and filters by tenant on read.

```mermaid
flowchart TB
  App[Application]
  App --> TenantRepo[TenantAwareBehaviorSpaceRepository]
  TenantRepo --> TenantProvider[ITenantProvider]
  TenantRepo --> InnerRepo[IBehaviorSpaceRepository]
  InnerRepo --> Db[(Database)]
  TenantProvider --> TenantId[Current TenantId]
  TenantRepo --> Metadata[SetMetadata TenantId on Save]
  TenantRepo --> Filter[Filter by TenantId on Get]
```

---

## Next step

If you're done here → [Setup](setup.md) or [Scenarios](scenarios.md).

---

## See also

- [Setup](setup.md) — Repository structure and samples
- [API Reference](api.md) — Main types and contracts
- [Advanced Features](advanced-features.md) — Similarity engines (time decay, source weights), vector normalization, rule-based and chained models, caching, clustering, events, analytics
