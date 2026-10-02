using System.Globalization;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.SystemOne;

/// <summary>
/// Uses a System One compatible engine as an <see cref="IIntentModel"/>: the behavior space
/// is sent as state, the configured intent catalog becomes one choice question, and the
/// engine's answer (choice + calibrated probabilities) is mapped to an <see cref="Intent"/>.
/// </summary>
public sealed class SystemOneIntentModel : IIntentModel
{
    private const string IntentQuestionId = "intent";

    private readonly SystemOneOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>Creates the model for the given engine options.</summary>
    public SystemOneIntentModel(SystemOneOptions options, HttpClient httpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        ArgumentNullException.ThrowIfNull(behaviorSpace);
        _options.Validate();

        if (_options.IntentCatalog.Count == 0)
            throw new InvalidOperationException(
                "SystemOneOptions.IntentCatalog must contain at least one intent (name → description).");

        var client = new SystemOneClient(_options, _httpClient);
        var response = client.Decide(BuildState(behaviorSpace), BuildQuestions());

        if (!response.Answers.TryGetValue(IntentQuestionId, out var answer))
        {
            return new Intent(
                Name: "unknown",
                Signals: Array.Empty<IntentSignal>(),
                Confidence: IntentConfidence.FromScore(0),
                Reasoning: "System One engine returned no answer for the intent question.");
        }

        var probabilities = answer.Probabilities;
        var name = answer.Choice ?? "unknown";
        var confidence = ResolveConfidence(answer, probabilities);
        var signals = BuildSignals(answer, probabilities, confidence);

        return new Intent(
            Name: name,
            Signals: signals,
            Confidence: IntentConfidence.FromScore(confidence),
            Reasoning: BuildReasoning(answer, probabilities, name, confidence));
    }

    private object BuildState(BehaviorSpace space)
    {
        var events = space.Events
            .Select(e => string.Create(CultureInfo.InvariantCulture, $"{e.Actor}:{e.Action}"))
            .ToArray();

        if (space.Metadata.Count == 0)
            return new Dictionary<string, object> { ["events"] = events };

        var state = new Dictionary<string, object> { ["events"] = events };
        state["metadata"] = space.Metadata.ToDictionary(m => m.Key, m => m.Value?.ToString() ?? "");
        return state;
    }

    private IReadOnlyDictionary<string, SystemOneQuestion> BuildQuestions() =>
        new Dictionary<string, SystemOneQuestion>
        {
            [IntentQuestionId] = SystemOneQuestion.Choice(
                _options.IntentQuestionInstructions,
                _options.IntentCatalog.ToDictionary(c => c.Key, c => (string?)c.Value)),
        };

    private static double ResolveConfidence(SystemOneAnswer answer, IReadOnlyDictionary<string, double>? probabilities)
    {
        var confidence = answer.AnswerConfidence
            ?? answer.Confidence
            ?? (probabilities is { Count: > 0 } ? probabilities.Values.Max() : 0.0);

        return Math.Clamp(confidence, 0.0, 1.0);
    }

    private IReadOnlyCollection<IntentSignal> BuildSignals(
        SystemOneAnswer answer,
        IReadOnlyDictionary<string, double>? probabilities,
        double confidence)
    {
        if (probabilities is null || probabilities.Count == 0)
        {
            return new[]
            {
                new IntentSignal(
                    Source: _options.Engine,
                    Description: answer.Choice ?? "no-answer",
                    Weight: confidence),
            };
        }

        return probabilities
            .Select(p => new IntentSignal(
                Source: _options.Engine,
                Description: p.Key,
                Weight: p.Value))
            .ToArray();
    }

    private static string BuildReasoning(
        SystemOneAnswer answer,
        IReadOnlyDictionary<string, double>? probabilities,
        string name,
        double confidence)
    {
        if (probabilities is null || probabilities.Count == 0)
            return $"choice={name}, confidence={confidence.ToString("0.00", CultureInfo.InvariantCulture)}";

        var parts = probabilities
            .OrderByDescending(p => p.Value)
            .Select(p => string.Create(
                CultureInfo.InvariantCulture,
                $"{p.Key}={p.Value.ToString("0.00", CultureInfo.InvariantCulture)}"));

        var answerType = answer.Score is not null ? "score" : answer.Noul is not null ? "noul" : "choice";
        return $"{answerType}: {string.Join(", ", parts)}";
    }
}
