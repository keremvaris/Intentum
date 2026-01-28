Intentum - Intent-Driven Development for the AI Era

[![CI](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml/badge.svg)](https://github.com/keremvaris/Intentum/actions/workflows/ci.yml)
[![NuGet Intentum.Core](https://img.shields.io/nuget/v/Intentum.Core.svg)](https://www.nuget.org/packages/Intentum.Core)
[![Coverage](https://keremvaris.github.io/Intentum/coverage/badge_linecoverage.svg)](https://keremvaris.github.io/Intentum/coverage/index.html)

Intentum replaces scenario-based BDD with behavior space inference.
It focuses on what the user was trying to do, not just what they did.

English | [Türkçe](README.tr.md)

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

Documentation
- GitHub Pages (EN/TR): https://keremvaris.github.io/Intentum/
- English docs: docs/en/index.md
- Turkish docs: docs/tr/index.md
  - Enable in GitHub: Settings -> Pages -> Source: GitHub Actions
- API reference (auto): https://keremvaris.github.io/Intentum/api/

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

Configuration (env vars)
- OPENAI_API_KEY, OPENAI_EMBEDDING_MODEL, OPENAI_BASE_URL
- GEMINI_API_KEY, GEMINI_EMBEDDING_MODEL, GEMINI_BASE_URL
- MISTRAL_API_KEY, MISTRAL_EMBEDDING_MODEL, MISTRAL_BASE_URL
- AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_EMBEDDING_DEPLOYMENT, AZURE_OPENAI_API_VERSION

Security
- Never commit API keys. Use environment variables or secret managers.
- Avoid logging raw requests/responses from providers in production.

Note
AI adapters are deterministic stubs in v1.0. Real HTTP calls are planned for v1.1.

Releasing and versioning
- **Versioning** is automatic from git tags via [MinVer](https://github.com/adamralph/minver) (`src/Directory.Build.props`). No manual version bump in code: tag `v1.0.1` and build → package version is `1.0.1`. Tag prefix is `v`; minimum major.minor is `1.0`.
- **Release script:** `chmod +x release.sh` then `./release.sh` — version is auto-bumped from the latest tag using conventional commits: **BREAKING** / `feat!:` → major, **feat:** → minor, **fix/docs/chore** → patch. Or run `./release.sh 1.0.1` to set the version manually.
- Pushing a tag like `v1.0.1` triggers GitHub Release and NuGet publish (see `.github/workflows/`).
- **Release notes** are generated from **conventional commits** via [git-cliff](https://git-cliff.org) (`cliff.toml`). Use prefixes: `feat:`, `fix:`, `docs:`, `chore:`, `ci:`, etc. If none are found, `CHANGELOG.md` is used as fallback.
- On every push to `master`, the **changelog** workflow updates `CHANGELOG.md` from conventional commits and commits it (with `[skip ci]`).

License
MIT
