# ADR-001: Why Intent Spaces Instead of Scenarios

## Status

Accepted

## Context

Traditional software testing uses linear scenarios (Given-When-Then). This works well for deterministic systems where:
- Every step is predictable
- Outages are binary (works/doesn't work)
- User behavior follows scripts

Modern systems are different:
- AI makes probabilistic decisions
- Users don't follow scripts
- Retries are normal, not exceptions
- "Edge cases" are expected behaviors

We need a testing paradigm that embraces uncertainty rather than fighting it.

## Decision

Use intent spaces (observed behavior → inferred intent) instead of scenario-based testing.

An intent space is a collection of observed behaviors that, when analyzed together, reveal what the system was trying to achieve.

## Alternatives Considered

1. **Enhanced BDD** - Add retry/uncertainty support to Given-When-Then
   - Rejected: Fundamentally linear model, can't represent intent

2. **Property-based testing** - Test properties rather than scenarios
   - Rejected: Good for algorithms, not for behavior interpretation

3. **Chaos engineering** - Inject failures to test resilience
   - Rejected: Complementary, but doesn't address intent inference

## Consequences

### Positive
+ More resilient to behavior variations
+ Better suited for AI/probabilistic systems
+ Enables confidence-based assertions
+ Tolerates alternative paths to same intent

### Negative
- Requires new mental model for developers
- Existing test suites need migration
- Less tooling support compared to BDD

### Neutral
* Can be used alongside existing BDD tests
* Incremental adoption possible

## Notes

The Intentum Manifesto (docs/en/manifesto.md) elaborates on this philosophy.
