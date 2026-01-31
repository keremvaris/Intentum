# Benchmark results (Intentum.Benchmarks)

This file is **generated** by running `./scripts/run-benchmarks.sh` from the repository root.
Commit this file after running the script to keep published results in sync.

---

```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a


```
| Method                        | Mean          | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------------------------------ |--------------:|----------:|----------:|-------:|-------:|----------:|
| ToVector_10Events             |     0.2650 ns | 0.0052 ns | 0.0049 ns |      - |      - |         - |
| ToVector_1KEvents             |     0.2544 ns | 0.0029 ns | 0.0027 ns |      - |      - |         - |
| ToVector_10KEvents            |     0.3319 ns | 0.0033 ns | 0.0029 ns |      - |      - |         - |
| LlmIntentModel_Infer_10Events |   273.7592 ns | 0.8363 ns | 0.7413 ns | 0.1326 |      - |    1112 B |
| LlmIntentModel_Infer_1KEvents | 1,695.3022 ns | 8.4526 ns | 7.9066 ns | 0.5703 | 0.0038 |    4784 B |
| PolicyEngine_Decide           |    12.8937 ns | 0.0204 ns | 0.0181 ns | 0.0029 |      - |      24 B |

---

See [Benchmarks](../en/benchmarks.md) for how to run and filter benchmarks. For improvement ideas and suggested solutions, see [Benchmarks — Improvement opportunities](../en/benchmarks.md#improvement-opportunities-and-suggested-solutions).
