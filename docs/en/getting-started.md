# Getting Started with Intentum

## What Intentum Actually Does

Intentum is a .NET framework for **intent-driven development**: observe user/system events, infer what they intend to do, and make decisions based on that intent.

```
BehaviorEvent → BehaviorSpace → BehaviorVector → Intent (with Confidence) → PolicyDecision
```

## Installation

```bash
# Core only (rules, policies, pipeline)
dotnet add package Intentum.Core
dotnet add package Intentum.Runtime

# With AI support (embeddings, LLM classification)
dotnet add package Intentum.AI
dotnet add package Intentum.AI.OpenAI  # or .Gemini, .Mistral, .AzureOpenAI, .Claude

# Everything in one package
dotnet add package Intentum.Providers
```

## Quick Start: Rule-Based Intent Detection

The simplest and most reliable approach - no AI needed:

```csharp
using Intentum.Core.Behavior;
using Intentum.Core.Models;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

// 1. Observe behavior events
var space = new BehaviorSpace();
space.Observe(new BehaviorEvent("user:123", "login.failed"));
space.Observe(new BehaviorEvent("user:123", "login.failed"));
space.Observe(new BehaviorEvent("user:123", "login.failed"));
space.Observe(new BehaviorEvent("user:123", "password.reset"));

// 2. Define rules to infer intent
var model = new RuleBasedIntentModel()
    .AddRule(space =>
    {
        var failedLogins = space.Events.Count(e => e.Action.Contains("login.failed"));
        var hasReset = space.Events.Any(e => e.Action.Contains("password.reset"));
        if (failedLogins >= 3 && hasReset)
            return new RuleMatch("AccountTakeover", 0.9, "Multiple failed logins + password reset");
        return null;
    });

// 3. Infer intent
var intent = model.Infer(space);
// intent.Name = "AccountTakeover", intent.Confidence.Score = 0.9

// 4. Define policy and decide
var policy = new IntentPolicyBuilder()
    .Block("HighRiskFraud", i => i.Confidence.Score >= 0.8)
    .Escalate("Suspicious", i => i.Confidence.Score >= 0.5)
    .Allow("Safe", _ => true)
    .Build();

var decision = intent.Decide(policy);
// decision = PolicyDecision.Block
```

## Cost-Efficient AI: ChainedIntentModel

Use rules first (fast + free), fall back to AI only when rules can't decide:

```csharp
using Intentum.Core.Models;
using Intentum.AI.Models;
using Intentum.AI.Mock;
using Intentum.AI.Similarity;

var rules = new RuleBasedIntentModel()
    .AddRule(space => { /* your rules */ return null; });

var llm = new LlmIntentModel(
    new MockEmbeddingProvider(),  // replace with OpenAIEmbeddingProvider for production
    new SimpleAverageSimilarityEngine());

var chained = new ChainedIntentModel(rules, llm);
var intent = chained.Infer(space);
// Rules tried first; if no match, LLM is called
```

## Pre-Built Rule Libraries

### Fraud Detection
```csharp
using Intentum.Core.Fraud;
using Intentum.Runtime.Fraud;

var model = new RuleBasedIntentModel();
foreach (var rule in FraudRules.AllRules())
    model.AddRule(rule);

var policy = FraudPolicies.Standard();
```

### E-Commerce Intent
```csharp
using Intentum.Core.Commerce;

var model = new RuleBasedIntentModel();
foreach (var rule in CommerceRules.AllRules())
    model.AddRule(rule);
```

### User Behavior Analytics
```csharp
using Intentum.Core.UBA;

var model = new RuleBasedIntentModel();
foreach (var rule in UserBehaviorRules.AllRules())
    model.AddRule(rule);
```

## AI-Powered Classification with Catalog

For real intent classification using embeddings (not just scoring):

```csharp
using Intentum.AI.Catalog;
using Intentum.AI.Models;

var catalog = new IntentCatalog()
    .Define("PurchaseIntent", "User wants to buy", "cart.add", "checkout.view", "payment.start")
    .Define("SupportIntent", "User needs help", "faq.view", "contact.click", "return.request")
    .Define("BrowsingIntent", "User is just looking", "product.view", "search", "category.browse");

// Resolve embeddings (one-time, cache the result)
await catalog.ResolveEmbeddingsAsync(embeddingProvider);

// Use catalog for classification
var model = new CatalogIntentModel(embeddingProvider, catalog);
var intent = model.Infer(space);
// Returns actual intent name from catalog, not "AI-Inferred-Intent"
```

## ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddIntentumPersistenceInMemory();
builder.Services.AddSingleton<IIntentEmbeddingProvider, MockEmbeddingProvider>();

// Authentication (optional)
builder.Services.AddIntentumAuth(opts =>
{
    opts.JwtSecret = "your-secret-key-min-32-chars-long!!";
    opts.ApiKeys.Add(new() { Key = "your-api-key", Name = "MyApp", Roles = ["Admin"] });
});

app.UseAuthentication();
app.UseAuthorization();
```

## Package Guide

| Package | Use Case |
|---------|----------|
| `Intentum.Core` | Rules, behavior spaces, intent models |
| `Intentum.Runtime` | Policies, decisions, rate limiting |
| `Intentum.AI` | Embeddings, similarity engines, AI inference |
| `Intentum.AI.OpenAI` | OpenAI embedding provider |
| `Intentum.AspNetCore` | Middleware, health checks, auth |
| `Intentum.Analytics` | Anomaly detection, confidence trends |
| `Intentum.Experiments` | A/B testing with statistical significance |
| `Intentum.Providers` | Meta-package: everything above |

## Try the sample

The **Intentum.Sample.Blazor** app demonstrates the same capabilities as the library: **infer** (rule-based and catalog/LLM), **policy** (Allow/Block/Warn/Escalate), **analytics** (Z-score/IQR anomaly, signals, export), and **experiments** (A/B test with p-value). What you see in the UI is built and used in production; event sources in the demos (e.g. "Demo Başlat") are simulated for illustration. See [samples/Intentum.Sample.Blazor/README.md](../../samples/Intentum.Sample.Blazor/README.md) and the Overview page in the app for "Gerçek / Simülasyon" (real vs demo).
