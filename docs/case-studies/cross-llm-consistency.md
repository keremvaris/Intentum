# Cross-LLM consistency (optional)

For the same **BehaviorSpace**, different embedding providers (OpenAI, Azure, Gemini, Mistral, etc.) can produce different **confidence scores** and, when using domain-specific models, different **intent names**. This page describes how to measure and document that variance.

## Why measure

- **Production choice:** When selecting a single provider for production, comparing confidence and intent output across providers on a fixed set of spaces helps you understand variance and pick a stable option.
- **Regression:** After changing a model or provider version, re-running the same spaces and comparing scores helps detect regressions.

## How to run (with real API keys)

1. **Pick a small set of BehaviorSpaces** (e.g. 10–20) that represent typical and edge cases (login flow, retries, abuse-like patterns).
2. **Run each space through each provider** you care about (e.g. OpenAI, Azure OpenAI, Gemini), using the same `LlmIntentModel` + `SimpleAverageSimilarityEngine` and only swapping `IIntentEmbeddingProvider`.
3. **Record per space:** intent name (if domain-specific), confidence score, and optionally level (High/Medium/Low/Certain).
4. **Summarize:** e.g. mean absolute difference in confidence score between provider A and B; or fraction of spaces where intent name agrees.

## Test in repo

The test `CrossLLMConsistencyTests.CrossProvider_SameSpace_TwoProviders_ProduceValidIntents` runs the same space through two mock providers and asserts both produce valid intents. It does not call real APIs. To run a real cross-LLM comparison, use the optional integration workflow (see [Embedding API error handling](../en/embedding-api-errors.md)) with multiple provider secrets and a small script or test that builds spaces, calls each provider, and writes a short report (e.g. to `docs/case-studies/`).

## Summary

| Goal                | Action                                                                 |
|---------------------|------------------------------------------------------------------------|
| Compare providers   | Same N spaces → run with provider A and B → record scores and intent  |
| Document variance   | Mean \|score_A − score_B\| or agreement rate on intent name            |
| Reproduce           | Fix the set of spaces and re-run when changing model or provider      |
