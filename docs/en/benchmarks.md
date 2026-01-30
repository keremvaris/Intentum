# Benchmarks

Intentum includes **BenchmarkDotNet** benchmarks for core operations: behavior space to vector, intent inference (with mock embedding), and policy decision. Use them to measure latency, throughput, and memory on your machine and to document performance for production sizing.

---

## What is benchmarked

| Benchmark | What it measures |
|-----------|------------------|
| **ToVector_10Events** / **ToVector_1KEvents** / **ToVector_10KEvents** | `BehaviorSpace.ToVector()` — building the behavior vector from 10, 1K, or 10K events. |
| **LlmIntentModel_Infer_10Events** / **LlmIntentModel_Infer_1KEvents** | `LlmIntentModel.Infer(space)` — full inference with **Mock** embedding provider (no API calls). |
| **PolicyEngine_Decide** | `intent.Decide(policy)` — policy evaluation (three rules). |

All benchmarks use the **Mock** embedding provider so runs do not require an API key and results are reproducible locally.

---

## Run benchmarks

From the repository root:

```bash
dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release
```

Results (Mean, Error, StdDev, Allocated) are printed to the console. BenchmarkDotNet also writes artifacts under `BenchmarkDotNet.Artifacts/results/`:

- **\*-report.html** — full report (open in browser).
- **\*-report-github.md** — Markdown table suitable for docs or README.
- **\*-report.csv** — raw numbers for further analysis.

### Filter a single benchmark

```bash
dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release -- --filter "*ToVector_10Events*"
```

---

## Updating benchmark results in docs

To refresh the benchmark results shown in the docs (e.g. [Case studies — Benchmark results](../case-studies/benchmark-results.md)):

```bash
./scripts/run-benchmarks.sh
```

This runs the benchmarks in Release and copies the generated `*-report-github.md` into `docs/case-studies/benchmark-results.md`. Commit that file to keep published results in sync with the codebase (optional; CI does not run benchmarks by default).

---

## Summary

| Item | Where |
|------|--------|
| Project | [benchmarks/Intentum.Benchmarks](../../benchmarks/) |
| Run | `dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release` |
| Artifacts | `BenchmarkDotNet.Artifacts/results/` |
| Refresh docs | `./scripts/run-benchmarks.sh` → `docs/case-studies/benchmark-results.md` |

For load testing the Sample.Web API (e.g. infer endpoint under concurrency), see [Load test: infer endpoint](../case-studies/load-test-infer.md).

---

## Improvement opportunities and suggested solutions

Based on benchmark results (LlmIntentModel allocation and latency scale with event/dimension count; PolicyEngine is already fast), these **concrete steps** can help:

| Goal | Solution |
|------|----------|
| **Reduce LlmIntentModel work for large event sets** | Call `space.ToVector(new ToVectorOptions(CapPerDimension: N))` and pass the result: `model.Infer(space, vector)`. That caps unique dimensions and reduces embedding calls. Or use the extension `model.Infer(space, toVectorOptions)` (see [Advanced Features](advanced-features.md)). |
| **Lower memory and API cost** | Use **CachedEmbeddingProvider** (or Redis/FusionCache) so repeated behavior keys do not call the API again; fewer allocations and latency for repeated dimensions. |
| **Keep inference latency low in production** | Use **ChainedIntentModel** (rule-based first, LLM fallback) so high-confidence paths skip the LLM; cap dimensions with ToVectorOptions; precompute and reuse vectors where the same space is evaluated multiple times. |
| **Larger datasets in production** | Run load tests (e.g. [Load test: infer endpoint](../case-studies/load-test-infer.md)) with realistic payload sizes; if p95 grows, cap dimensions or add caching before increasing capacity. |
| **PolicyEngine** | No change needed; already in the tens of nanoseconds. |
