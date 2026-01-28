Intentum - Intent-Driven Development for the AI Era

[![CI](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml/badge.svg)](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml)
[![NuGet Intentum.Core](https://img.shields.io/nuget/v/Intentum.Core.svg)](https://www.nuget.org/packages/Intentum.Core)

Intentum replaces scenario-based BDD with behavior space inference.
It focuses on what the user was trying to do, not just what they did.

Why Intentum?
- Non-deterministic flows are now common.
- AI-driven systems adapt and drift.
- Assertions are too rigid; intent is more realistic.

When not to use
- Fully deterministic, low-variance systems with stable requirements.
- Small scripts or one-off utilities where behavior drift is irrelevant.

BDD vs Intentum
- BDD: Scenario-driven, deterministic, pass/fail
- Intentum: Behavior-driven, probabilistic, policy decisions

Core Concepts
Behavior Space
Observe behavior instead of writing scenarios.

Intent Inference
Infer intent from the behavior space and compute confidence.

Policy Decisions
Replace pass/fail with policy-driven decisions.

Quick Example
```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime.Policy;

var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "submit");

var intentModel = new LlmIntentModel(
    new MockEmbeddingProvider(),
    new SimpleAverageSimilarityEngine());

var intent = intentModel.Infer(space);

var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "HighConfidenceAllow",
        i => i.Confidence.Level is "High" or "Certain",
        PolicyDecision.Allow));

var decision = intent.Decide(policy);
```

Run the sample
```bash
dotnet run --project samples/Intentum.Sample
```

Showcase output (trimmed)
```
=== INTENTUM SCENARIO: PaymentHappyPath ===
Events            : 2
Intent Confidence : High
Decision          : Allow
Behavior Vector:
 - user:login = 1
 - user:submit = 1

=== INTENTUM SCENARIO: PaymentWithRetries ===
Events            : 4
Intent Confidence : Medium
Decision          : Observe
Behavior Vector:
 - user:login = 1
 - user:retry = 2
 - user:submit = 1

=== INTENTUM SCENARIO: SuspiciousRetries ===
Events            : 4
Intent Confidence : Medium
Decision          : Block
Behavior Vector:
 - user:login = 1
 - user:retry = 3
```

Packages
- Intentum.Core
- Intentum.Runtime
- Intentum.AI
- Intentum.AI.OpenAI
- Intentum.AI.Gemini
- Intentum.AI.Claude
- Intentum.AI.Mistral
- Intentum.AI.AzureOpenAI

Note
AI adapters are deterministic stubs in v1.0. Real HTTP calls are planned for v1.1.

License
MIT
