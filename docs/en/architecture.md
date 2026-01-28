# Architecture (EN)

This page describes Intentum’s architecture: core flow, package layout, inference pipeline, and optional extensions. Diagrams use Mermaid.

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
| **Observe** | Record what happened: `space.Observe(actor, action)` or `BehaviorSpaceBuilder`. Events form a **BehaviorSpace**. |
| **Infer** | **IIntentModel** (e.g. LlmIntentModel) uses embeddings + similarity engine → **Intent** (name, confidence, signals). |
| **Decide** | **IntentPolicy** evaluates rules in order → **PolicyDecision** (Allow, Observe, Warn, Block, Escalate, RequireAuth, RateLimit). |

---

## Package architecture

Packages are grouped by responsibility: core types, runtime (policy + rate limiting), AI (model + embeddings + caching), persistence, analytics, and optional extensions.

```mermaid
flowchart TB
  subgraph core [Core]
    CorePkg[Intentum.Core]
    CorePkg --> Behavior[BehaviorSpace, BehaviorEvent, BehaviorSpaceBuilder]
    CorePkg --> Intent[Intent, IntentConfidence, IntentSignal]
    CorePkg --> Batch[IBatchIntentModel, BatchIntentModel]
    CorePkg --> Model[IIntentModel]
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
    AIPkg --> Similarity[IIntentSimilarityEngine, SimpleAverage, Cosine, Composite]
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
    Persist --> Repos[IBehaviorSpaceRepository, IIntentHistoryRepository]
    PersistEF --> Repos
  end

  subgraph extensions [Extensions]
    AspNet[Intentum.AspNetCore]
    Analytics[Intentum.Analytics]
    Clustering[Intentum.Clustering]
    Events[Intentum.Events]
    Experiments[Intentum.Experiments]
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
  ai --> Redis
```

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
  Model->>Space: Events / ToVector
  Model->>Embed: Embed(behaviorKey)
  Embed->>Cache: Get(key)
  alt cache miss
    Embed->>Embed: Call underlying provider
    Embed->>Cache: Set(key, embedding)
  end
  Embed-->>Model: IntentEmbedding
  Model->>Sim: CalculateIntentScore(embeddings)
  Sim-->>Model: score
  Model-->>App: Intent
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
    WebApp[Sample.Web]
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

## See also

- [Setup](setup.md) — Repository structure and samples
- [API Reference](api.md) — Main types and contracts
- [Advanced Features](advanced-features.md) — Caching, clustering, events, analytics
