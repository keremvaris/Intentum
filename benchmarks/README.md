# Intentum benchmarks

BenchmarkDotNet benchmarks for **Core** (BehaviorSpace.ToVector), **AI** (LlmIntentModel.Infer with mock embedding), and **Runtime** (PolicyEngine.Decide). No API key required; uses Mock embedding provider.

## Run

From repo root:

```bash
dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release
```

Results (Mean, Error, StdDev, Allocated) are printed to the console. Artifacts are written under `BenchmarkDotNet.Artifacts/results/`:

- **\*-report.html** — full report (open in browser)
- **\*-report-github.md** — Markdown table for docs
- **\*-report.csv** — raw numbers

### Filter a single benchmark

```bash
dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release -- --filter "*ToVector_10Events*"
```

## Update docs with latest results

To refresh the benchmark results in the documentation (e.g. [docs/case-studies/benchmark-results.md](../docs/case-studies/benchmark-results.md)):

```bash
./scripts/run-benchmarks.sh
```

This runs the benchmarks in Release and copies the generated Markdown report to `docs/case-studies/benchmark-results.md`. Commit that file to keep published results in sync.

## Documentation

- [Benchmarks (EN)](../docs/en/benchmarks.md) — full guide: what is benchmarked, how to run, how to update docs.
- [Case studies](../docs/case-studies/README.md) — greenwashing metrics, load test, cross-LLM consistency, benchmark results.
