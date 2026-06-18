# ADR-004: Why Behavior Space as List

## Status

Accepted

## Context

Behavior spaces represent observed events. We need to choose a data structure that:
- Preserves event order (temporal sequence matters)
- Supports efficient vector conversion
- Is simple to serialize/deserialize

## Decision

Use a simple `List<BehaviorEvent>` for storing events in `BehaviorSpace`.

Events are appended in observation order and converted to vectors by counting actor:action pairs.

## Alternatives Considered

1. **Graph structure** - Model events as a graph with edges
   - Rejected: Overkill for most use cases, complex serialization

2. **Time-series database** - Use specialized time-series storage
   - Rejected: Adds dependency, premature optimization

3. **Event store** - Append-only log with projections
   - Rejected: Good for event sourcing, but too complex for basic use

## Consequences

### Positive
+ Simple to implement and understand
+ Efficient vector conversion (O(n))
+ Easy to serialize (JSON)
+ Supports time window queries

### Negative
- No built-in deduplication
- No compaction/summarization
- Memory grows with event count

### Neutral
* Can be extended to graph/event store later
* Windowing provides natural compaction

## Notes

The `ToVector()` method converts the list to a vector efficiently.
