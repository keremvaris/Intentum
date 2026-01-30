# Intent explainability

Intentum provides **signal-based explainability** (which behaviors contributed and by how much) and optional **reasoning** (short text from the model or rule).

## Signal contributions

[IntentExplainer](api.md) computes **signal contributions**: for each signal (behavior dimension) in the intent, the share of total weight (as a percentage). Use `GetSignalContributions(intent)` to get a list of `(Source, Description, Weight, ContributionPercent)` ordered by contribution.

**Use case:** "Why did the model infer this intent?" — show the top N signals (e.g. `user:login.failed`, `user:retry`) and their percentages.

## Human-readable explanation

`GetExplanation(intent, maxSignals)` returns a single string: intent name, confidence level/score, top signal contributors, and **reasoning** (if `Intent.Reasoning` is set).

- **Reasoning:** When your intent model (e.g. Claude message model, or a rule-based model) sets `Intent.Reasoning`, the explainer appends it to the explanation. Example: GreenwashingIntentModel sets `Reasoning` to a short rationale (e.g. "N sinyal; ağırlıklı skor X → IntentName"). LLM-based models can set a short "because …" sentence.
- **Signal sentences:** For the top N signals, the explainer already turns them into readable text (e.g. "user:login.failed (25%); user:retry (20%)"). You can extend this by mapping `Description` (actor:action) to a human sentence (e.g. "Failed login" instead of "user:login.failed") in your UI or logging layer.

## Sample usage

See Sample.Web: `POST /api/intent/explain` returns signal contributions and explanation text. The response includes top signals and, when the model provides it, reasoning.

## Summary

| Feature           | Where                         | Use |
|-------------------|-------------------------------|-----|
| Signal contributions | `IIntentExplainer.GetSignalContributions` | Which behaviors contributed and by how much |
| Explanation text  | `IIntentExplainer.GetExplanation` | Single string: name, confidence, top signals, reasoning |
| Reasoning         | `Intent.Reasoning` (set by model) | Short "why" from rule or LLM; included in explanation when present |
