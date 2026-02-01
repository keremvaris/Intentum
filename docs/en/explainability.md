# Intent explainability

Intentum provides **signal-based explainability** (which behaviors contributed and by how much), **rule trace / reasoning** (short text from the model or rule), and **confidence breakdown** (score and level). The explanation API unifies these in a single response.

## Explanation API response (feature contribution + rule trace + confidence breakdown)

`POST /api/intent/explain` (Sample Web) returns a single JSON object:

| Field | Description |
|-------|-------------|
| **IntentName** | Inferred intent name. |
| **Confidence** | Confidence level (e.g. High, Medium, Low). |
| **ConfidenceScore** | Numeric confidence score (0–1). |
| **Reasoning** | Rule trace or short "why" from the model (e.g. which rule matched, or "Fallback: LLM"). |
| **Explanation** | Human-readable summary: intent, confidence, top signal contributors, and reasoning. |
| **SignalContributions** | Per-signal contribution: Source, Description, Weight, ContributionPercent (ordered by contribution). |

Use **SignalContributions** for feature contribution bars; **Reasoning** for rule trace; **Confidence** and **ConfidenceScore** for confidence breakdown. The **Explanation** string is the full readable summary.

## Signal contributions

[IntentExplainer](api.md) computes **signal contributions**: for each signal (behavior dimension) in the intent, the share of total weight (as a percentage). Use `GetSignalContributions(intent)` to get a list of `(Source, Description, Weight, ContributionPercent)` ordered by contribution.

**Use case:** "Why did the model infer this intent?" — show the top N signals (e.g. `user:login.failed`, `user:retry`) and their percentages.

## Human-readable explanation

`GetExplanation(intent, maxSignals)` returns a single string: intent name, confidence level/score, top signal contributors, and **reasoning** (if `Intent.Reasoning` is set).

- **Reasoning:** When your intent model (e.g. Claude message model, or a rule-based model) sets `Intent.Reasoning`, the explainer appends it to the explanation. Example: GreenwashingIntentModel sets `Reasoning` to a short rationale (e.g. "N sinyal; ağırlıklı skor X → IntentName"). LLM-based models can set a short "because …" sentence.
- **Signal sentences:** For the top N signals, the explainer already turns them into readable text (e.g. "user:login.failed (25%); user:retry (20%)"). You can extend this by mapping `Description` (actor:action) to a human sentence (e.g. "Failed login" instead of "user:login.failed") in your UI or logging layer.

## Intent tree (decision tree)

**IIntentTreeExplainer** builds a **decision tree** for the policy path: which rule matched, signal nodes, and intent summary. Use it when you need to show *why* a policy returned Allow/Block in a tree form (e.g. for audit or UI).

- **IntentTreeExplainer.ExplainTree(intent, policy, behaviorSpace?)** returns **IntentDecisionTree** (IntentSummary, SignalNodes, MatchedRule).
- Sample Web: `POST /api/intent/explain-tree` (same body as infer) returns the tree JSON.

See [Advanced Features – Intent Tree](advanced-features.md#intent-tree) for setup and options.

## Sample usage

See Sample.Blazor: `POST /api/intent/explain` returns signal contributions and explanation text. The response includes top signals and, when the model provides it, reasoning. Use `POST /api/intent/explain-tree` for the policy decision tree.

## Summary

| Feature           | Where                         | Use |
|-------------------|-------------------------------|-----|
| Signal contributions | `IIntentExplainer.GetSignalContributions` | Which behaviors contributed and by how much |
| Explanation text  | `IIntentExplainer.GetExplanation` | Single string: name, confidence, top signals, reasoning |
| Intent tree       | `IIntentTreeExplainer.ExplainTree` | Decision tree: matched rule, signal nodes; Sample: `POST /api/intent/explain-tree` |
| Reasoning         | `Intent.Reasoning` (set by model) | Short "why" from rule or LLM; included in explanation when present |

**Next step:** When you're done with this page → [Advanced features](advanced-features.md) or [What these features do](features-simple-guide.md).
