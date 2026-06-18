# ADR-003: Why Static Confidence Thresholds

## Status

Accepted

## Context

Intent inference produces a confidence score (0-1). We need to map this to actionable levels (Low, Medium, High, Certain) for policy decisions.

Dynamic calibration requires historical data and complex fitting algorithms. For v1.0, we need a simpler approach.

## Decision

Use static thresholds for confidence levels:
- Low: < 0.3
- Medium: 0.3 - 0.6
- High: 0.6 - 0.85
- Certain: >= 0.85

These thresholds are defined in `IntentConfidence.FromScore()` and can be overridden via `IConfidenceCalculator`.

## Alternatives Considered

1. **Dynamic calibration** - Adjust thresholds based on observed outcomes
   - Deferred to post-v1.0 (listed on roadmap as "confidence calibration")

2. **Per-domain thresholds** - Different thresholds for different domains
   - Deferred: Adds complexity, can be added later via IConfidenceCalculator

3. **No thresholds** - Use raw score only
   - Rejected: Policies need categorical decisions

## Consequences

### Positive
+ Simple to understand and implement
+ Deterministic behavior
+ No historical data required
+ Easy to override via IConfidenceCalculator

### Negative
- May not be optimal for all domains
- Requires manual tuning for specific use cases
- Static thresholds can become outdated

### Neutral
* Can be replaced with dynamic calibration later
* Default thresholds work for most cases

## Notes

The roadmap includes "confidence calibration" as a post-v1.0 depth goal.
