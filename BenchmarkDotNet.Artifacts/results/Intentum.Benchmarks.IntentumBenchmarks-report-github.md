```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.2 (10.0.2, 10.0.225.61305), Arm64 RyuJIT armv8.0-a


```
| Method                        | Mean          | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------------------------------ |--------------:|----------:|----------:|-------:|-------:|----------:|
| ToVector_10Events             |     0.2719 ns | 0.0335 ns | 0.0314 ns |      - |      - |         - |
| ToVector_1KEvents             |     0.2019 ns | 0.0083 ns | 0.0073 ns |      - |      - |         - |
| ToVector_10KEvents            |     0.3107 ns | 0.0211 ns | 0.0187 ns |      - |      - |         - |
| LlmIntentModel_Infer_10Events |   301.2635 ns | 1.3713 ns | 1.1451 ns | 0.1326 |      - |    1112 B |
| LlmIntentModel_Infer_1KEvents | 1,700.0173 ns | 6.5487 ns | 6.1257 ns | 0.5703 | 0.0038 |    4784 B |
| PolicyEngine_Decide           |    14.2151 ns | 0.1076 ns | 0.0953 ns | 0.0029 |      - |      24 B |
