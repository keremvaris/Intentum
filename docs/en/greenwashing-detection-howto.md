# Greenwashing detection (how-to)

Detect sustainability-report greenwashing using Intentum: behavior space, intent inference, and policy decisions.

---

## The problem (classic approach)

Classic systems do:

```
IF report.Contains("sustainable") AND !report.Contains("data") THEN flag_greenwashing
IF report.Contains("green") AND report.Contains("images") AND !report.Contains("metrics") THEN suspicious
```

Issues:

- Keyword-based rules break when wording changes
- Binary outcome (suspicious / not) ignores degree of risk
- No link between detected patterns and recommended actions
- Hard to adapt to new greenwashing techniques

---

## The question Intentum asks

What is the company trying to communicate? Genuine sustainability effort, or the appearance of it?

That is not a binary question. Intentum infers intent and confidence from observed behavioral signals.

---

## 1. Behavior space from report text

Collect signals from the sustainability report and record them as behavior events. Use `BehaviorSpace` with the `Observe(actor, action)` extension.

**Actor** = signal category (e.g. `"language"`, `"data"`, `"imagery"`).  
**Action** = specific signal (e.g. `"claim.vague"`, `"metrics.without.proof"`).

Example: vague claims, unsubstantiated comparisons, metrics without proof, favorable baseline.

```csharp
using System.Text.RegularExpressions;
using Intentum.Core;
using Intentum.Core.Behavior;

var space = new BehaviorSpace();
var regexTimeout = TimeSpan.FromSeconds(1);

// Vague sustainability language
foreach (var pattern in new[] { "sustainable future", "green transition", "eco-friendly", "clean production" })
{
    if (report.Contains(pattern))
    {
        var count = Regex.Matches(report, Regex.Escape(pattern), RegexOptions.IgnoreCase, regexTimeout).Count;
        for (int i = 0; i < count; i++)
            space.Observe("language", "claim.vague");
    }
}

// Unsubstantiated comparative claims
if (HasComparativeClaims(report))
    space.Observe("language", "comparison.unsubstantiated");

// Metrics without verification (ISO, audit, verified)
var hasMetrics = Regex.IsMatch(report, @"%\s*(reduction|increase)|(\d+\s*(ton|kg|kWh|CO2))", RegexOptions.IgnoreCase, regexTimeout);
var hasProof = report.Contains("ISO") || report.Contains("verified") || report.Contains("audit");
if (hasMetrics && !hasProof)
    space.Observe("data", "metrics.without.proof");

// Favorable baseline selection
if (UsesFavorableBaseline(report))
    space.Observe("data", "baseline.manipulation");

// Nature imagery without supporting data
if (HasNatureImageryWithoutData(report))
    space.Observe("imagery", "nature.without.data");
```

Use `space.Events` to read back events; `space.Events.Count` is the total number of signals. For event-level metadata (e.g. pattern text, count), use `BehaviorSpaceBuilder` and `BehaviorEvent(actor, action, occurredAt, metadata)`.

---

## 2. Intent model

Implement `IIntentModel` to map the behavior space to a named intent and confidence. Options:

- **Rule-based:** Use `RuleBasedIntentModel` with rules that inspect `behaviorSpace.Events` (e.g. count events per actor:action) and return `RuleMatch(Name, Score, Reasoning)`. The built-in model turns the first matching rule into an `Intent` with `Signals` derived from the behavior vector.
- **Custom:** Implement `IIntentModel.Infer(BehaviorSpace, BehaviorVector?)` yourself: aggregate greenwashing signals, compute a weight/score, pick intent name (e.g. ActiveGreenwashing, StrategicObfuscation, SelectiveDisclosure, UnintentionalMisrepresentation, GenuineSustainability), and return `Intent(Name, Signals, Confidence, Reasoning)`.

Carry "detected patterns" in `Intent.Signals`: e.g. `new IntentSignal("greenwashing", "language:claim.vague", weight)`. Use `Reasoning` for a short human-readable summary.

Example intent categorization (custom logic):

- Weight &gt; 0.8 → `ActiveGreenwashing`
- Weight &gt; 0.6 → `StrategicObfuscation`
- Weight &gt; 0.4 → `SelectiveDisclosure`
- Weight &gt; 0.2 → `UnintentionalMisrepresentation`
- Else → `GenuineSustainability`

Confidence: use `IntentConfidence.FromScore(score)`; `Level` is the string `"Low"`, `"Medium"`, `"High"`, or `"Certain"`.

---

## 3. Policy

Use `IntentPolicyBuilder` to map intent name and confidence to a `PolicyDecision`. Intentum does not define custom actions like "IMMEDIATE_REVIEW"; you map to the enum and interpret it in your application.

Suggested mapping:

- **Critical risk:** `ActiveGreenwashing` and high confidence → `Block` or `Escalate`
- **Needs verification:** `StrategicObfuscation` or `SelectiveDisclosure` with medium+ confidence → `Warn` or `RequireAuth`
- **Monitor:** Lower risk but non-zero confidence → `Observe`
- **Acceptable:** `GenuineSustainability` or low confidence → `Allow`

```csharp
using Intentum.Runtime;
using Intentum.Runtime.Policy;

var policy = new IntentPolicyBuilder()
    .Escalate("CriticalGreenwashing",
        i => i.Name == "ActiveGreenwashing" && i.Confidence.Score >= 0.7)
    .Warn("NeedsVerification",
        i => i.Name == "StrategicObfuscation" ||
             (i.Name == "SelectiveDisclosure" && i.Confidence.Score >= 0.5))
    .Observe("Monitor",
        i => i.Confidence.Score > 0.3)
    .Allow("LowRisk", _ => true)
    .Build();

var decision = intent.Decide(policy);
```

Map `decision` (e.g. `Escalate`, `Warn`, `Observe`) plus `intent.Name` and `intent.Signals` in your app to concrete actions (e.g. "immediate review", "third-party audit", "quarterly monitoring").

---

## 4. Solution layer (application-level)

Intentum does not provide solution DTOs. Build a small layer that takes `Intent`, `BehaviorSpace`, and `PolicyDecision` and produces your own "solution package" (urgent actions, communication fixes, verification steps). Use `intent.Name`, `intent.Signals`, and `intent.Confidence` to drive the logic; use `space.Events` if you need raw signals.

Example structure:

- If `intent.Name == "ActiveGreenwashing"` and `decision == PolicyDecision.Escalate`: add urgent actions (e.g. suspend claims, internal review, publish supporting data).
- If signals contain `"data:metrics.without.proof"`: add "publish supporting data for all metrics".
- If signals contain `"data:baseline.manipulation"`: add "recalculate using industry-standard baseline".

---

## 5. Summary

| Approach | Intentum |
|----------|----------|
| Keyword / rule matching | Behavior space + intent inference |
| Binary (yes/no) | Confidence score and level |
| Fixed rules | Policy maps intent + confidence → decision |
| Opaque | Signals and reasoning explain the intent |

You get flexibility (new signals = new observations), a confidence score instead of a binary flag, and explainability via `Intent.Signals` and `Reasoning`. The policy stays separate from the intent model; you can change rules without retraining.

---

## 6. Sample application (Intentum.Sample.Web)

The web sample includes a full greenwashing flow in the UI and HTTP API.

**Run:**

```bash
dotnet run --project samples/Intentum.Sample.Web
```

- **UI:** http://localhost:5000/ → **Örnekler** → **Greenwashing tespiti**
- **API docs:** http://localhost:5000/scalar

**Endpoints:**

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/greenwashing/analyze` | Analyze report text (optional: `sourceType`, `language`, `imageBase64`). Returns intent, decision, signals, suggested actions, `blockchainRef`, `sourceMetadata`, `visualResult`. |
| GET | `/api/greenwashing/recent?limit=15` | Last greenwashing analyses (in-memory; used by Dashboard). |

**Request body (POST /api/greenwashing/analyze):**

```json
{
  "report": "Sustainability report text...",
  "sourceType": "Report",
  "language": "tr",
  "imageBase64": null
}
```

`sourceType`: `"Report"`, `"SocialMedia"`, `"PressRelease"`, `"InvestorPresentation"` (mock metadata).  
`language`: `"tr"`, `"en"`, `"de"` or empty (all pattern sets).  
`imageBase64`: optional; if set, a green-dominance score is computed and may add `imagery:nature.without.data` to the behavior space.

**Response:** `intentName`, `confidence`, `confidenceScore`, `decision`, `signalDescriptions`, `suggestedActions`, `blockchainRef`, `sourceMetadata` (with optional `scope3Summary`), `visualResult` (when image was sent).

**Demo features in the sample:**

- **Multi-language:** TR, EN, DE pattern sets via `SustainabilityReporter.AnalyzeReport(report, language)`.
- **Visual (demo):** Image upload → green score; if above threshold, adds imagery signal.
- **Scope 3 (mock):** Fixed supplier list; `scope3Summary` in metadata for Press/Investor source types.
- **Blockchain (mock):** Every analysis returns a unique `blockchainRef` (e.g. `0x…`).
- **Recent analyses:** Dashboard tab shows "Son greenwashing analizleri" from `GET /api/greenwashing/recent`; list refreshes when Dashboard is open; mock entries are added every 30 seconds.

Implementation lives under `samples/Intentum.Sample.Web/Features/GreenwashingDetection/`: `SustainabilityReporter`, `GreenwashingIntentModel`, `SustainabilitySolutionGenerator`, `GreenwashingImageAnalyzer`, `GreenwashingScope3Mock`, `GreenwashingRecentStore`.

---

## See also

- [Real-world scenarios](real-world-scenarios.md) — Fraud/abuse and AI fallback
- [Designing intent models](designing-intent-models.md) — Heuristic vs weighted vs LLM
- [Setup](setup.md) — Build and run Sample.Web
- Example: [examples/greenwashing-intent](https://github.com/keremvaris/Intentum/tree/master/examples/greenwashing-intent) — Console greenwashing sample (no API)
