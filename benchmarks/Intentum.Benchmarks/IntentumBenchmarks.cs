using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Benchmarks;

[MemoryDiagnoser]
public class IntentumBenchmarks
{
    private BehaviorSpace _space10 = null!;
    private BehaviorSpace _space1K = null!;
    private BehaviorSpace _space10K = null!;
    private LlmIntentModel _model = null!;
    private IntentPolicy _policy = null!;
    private Intent _intent = null!;

    [GlobalSetup]
    public void Setup()
    {
        _space10 = new BehaviorSpace();
        for (var i = 0; i < 10; i++)
            _space10.Observe("user", $"action.{i % 3}");

        _space1K = new BehaviorSpace();
        for (var i = 0; i < 1000; i++)
            _space1K.Observe("user", $"action.{i % 20}");

        _space10K = new BehaviorSpace();
        for (var i = 0; i < 10000; i++)
            _space10K.Observe("user", $"action.{i % 50}");

        _model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        _intent = _model.Infer(_space10);

        _policy = new IntentPolicy()
            .AddRule(new PolicyRule("BlockRetry", i => i.Signals.Count(s => s.Description.Contains("retry")) >= 3, PolicyDecision.Block))
            .AddRule(new PolicyRule("AllowHigh", i => i.Confidence.Level is "High" or "Certain", PolicyDecision.Allow))
            .AddRule(new PolicyRule("Observe", _ => true, PolicyDecision.Observe));
    }

    [Benchmark]
    public BehaviorVector ToVector_10Events() => _space10.ToVector();

    [Benchmark]
    public BehaviorVector ToVector_1KEvents() => _space1K.ToVector();

    [Benchmark]
    public BehaviorVector ToVector_10KEvents() => _space10K.ToVector();

    [Benchmark]
    public Intent LlmIntentModel_Infer_10Events() => _model.Infer(_space10);

    [Benchmark]
    public Intent LlmIntentModel_Infer_1KEvents() => _model.Infer(_space1K);

    [Benchmark]
    public PolicyDecision PolicyEngine_Decide() => _intent.Decide(_policy);
}

public static class Program
{
    public static void Main(string[] args) =>
        BenchmarkSwitcher.FromAssembly(typeof(IntentumBenchmarks).Assembly).Run(args);
}
