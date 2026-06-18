using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;

namespace Intentum.Benchmarks;

[Config(typeof(RegressionConfig))]
[MemoryDiagnoser]
public class BenchmarkRegression
{
    private BehaviorSpace _space1K = null!;
    private LlmIntentModel _model = null!;
    private Intent _intent = null!;
    private BehaviorSpace _policySpace = null!;

    [GlobalSetup]
    public void Setup()
    {
        _space1K = new BehaviorSpace();
        for (var i = 0; i < 1000; i++)
            _space1K.Observe("user", $"action.{i % 20}");

        _model = new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());
        _intent = _model.Infer(_space1K);

        _policySpace = new BehaviorSpace();
        for (var i = 0; i < 100; i++)
            _policySpace.Observe("user", $"action.{i % 10}");
    }

    [Benchmark(Baseline = true)]
    public BehaviorVector ToVector_Baseline() => _space1K.ToVector();

    [Benchmark]
    public Intent LlmIntentModel_Infer() => _model.Infer(_space1K);

    private class RegressionConfig : ManualConfig
    {
        public RegressionConfig()
        {
            AddJob(Job.ShortRun.WithWarmupCount(3).WithIterationCount(5));
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
        }
    }
}