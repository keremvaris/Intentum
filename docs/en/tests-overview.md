# Tests Overview

**Why you're reading this page:** This page gives a short overview of Intentum test projects (Intentum.Tests, Intentum.Tests.Integration): what is covered, how to run them, and how they relate to samples and examples. It is the right place if you are asking "What does each test do?"

This page gives a short overview of Intentum tests: what is covered, how to run them, and how they relate to **samples** and **examples**.

---

## Test projects

| Project | Description |
|--------|-------------|
| **Intentum.Tests** | Unit and contract tests: BehaviorSpace, inference, policy, providers (mock HTTP), clustering, explainability, simulation, versioning, multi-tenancy, events, experiments. No real API keys. |
| **Intentum.Tests.Integration** | Integration tests: greenwashing case study (accuracy/F1 on labeled data). Optional: real APIs if env vars set. |

---

## How to run

From the repository root:

```bash
# All unit tests (no API keys)
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj

# Exclude provider integration tests (OpenAI, Azure, Gemini) when no keys
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName!=Intentum.Tests.OpenAIIntegrationTests&FullyQualifiedName!=Intentum.Tests.AzureOpenAIIntegrationTests&FullyQualifiedName!=Intentum.Tests.GeminiIntegrationTests&FullyQualifiedName!=Intentum.Tests.MistralIntegrationTests"

# Integration tests
dotnet test tests/Intentum.Tests.Integration/Intentum.Tests.Integration.csproj
```

See [Testing](testing.md) for details and [Local integration tests](local-integration-tests.md) for scripts.

---

## What is covered (summary)

- **Core:** BehaviorSpace, ToVector, intent confidence, policy engine (Evaluate, EvaluateWithRule), rate limit, localization; intent resolution pipeline; behavior space sanitization (anonymization/masking).
- **Models:** Rule-based, chained, multi-stage, sliding window, LLM (mock + provider parsing with mock HTTP); ONNX intent model (constructor/options).
- **Analytics:** IntentAnalytics (trends, distribution, anomalies, timeline, export, intent graph snapshot).
- **Explainability:** IntentExplainer, IntentTreeExplainer, decision tree.
- **Observability:** Policy execution log (DecideWithExecutionLog), metrics.
- **Persistence:** In-memory repo (history + behavior space), EF/Redis/Mongo not required for unit tests.
- **Simulation:** BehaviorSpaceSimulator, ScenarioRunner.
- **Streaming:** BoundedMemoryBehaviorStreamConsumer, WindowedBatchBuffer.
- **Patterns:** BehaviorPatternDetector, template matching.
- **Policy store:** FilePolicyStore, SafeConditionBuilder (declarative rules).

Integration tests cover greenwashing accuracy/F1 on labeled data; see [Case study — Greenwashing metrics](../case-studies/greenwashing-metrics.md).

---

## Tests vs samples vs examples

| | Tests | Examples | Samples |
|--|--------|----------|---------|
| **Purpose** | Assert contracts and core behavior; no real APIs in CI. | Learn one use case; copy-paste friendly. | Full app: many features, Web API, UI. |
| **Run** | `dotnet test tests/Intentum.Tests` | `dotnet run --project examples/<name>` | `dotnet run --project samples/Intentum.Sample.Blazor` |
| **Docs** | [Testing](testing.md), this page | [Examples overview](examples-overview.md) | [API](api.md), [Setup](setup.md) |

---

## Adding tests

- Use **mock HttpClient** or in-memory providers so tests don’t call real APIs.
- For new features (e.g. timeline, intent tree, pattern detector), add tests in `Intentum.Tests` and extend the “What is covered” list in [Testing](testing.md).

**Next step:** When you're done with this page → [Testing](testing.md) or [Examples overview](examples-overview.md).
