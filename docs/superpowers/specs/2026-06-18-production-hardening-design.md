# Production Hardening Design Spec

## Goal

Add 5 resilience patterns (Circuit Breaker, Retry, Bulkhead, Degradation, Timeout) to the Intentum runtime for production-grade reliability.

## Context

Intentum currently has ad-hoc resilience implementations embedded in specific providers (FallbackEmbeddingProvider has an inline circuit breaker; 4 separate retry implementations exist with no shared abstraction). The RateLimiting pattern at `src/Intentum.Runtime/RateLimiting/` serves as the established reference: interface + options record + in-memory implementation + DI extension.

## Architecture

```
src/Intentum.Runtime/Resilience/
├── CircuitBreaker/
│   ├── ICircuitBreaker.cs         — Interface + enums
│   ├── MemoryCircuitBreaker.cs    — Default implementation
│   └── CircuitBreakerExtensions.cs— DI registration
├── Retry/
│   ├── IRetryPolicy.cs            — Interface + enums
│   ├── MemoryRetryPolicy.cs       — Default implementation
│   └── RetryExtensions.cs         — DI registration
├── Bulkhead/
│   ├── IBulkhead.cs               — Interface + options
│   ├── MemoryBulkhead.cs          — Default implementation
│   └── BulkheadExtensions.cs      — DI registration
├── Degradation/
│   ├── IDegradationPolicy.cs      — Interface + options
│   ├── MemoryDegradationPolicy.cs — Default implementation
│   └── DegradationExtensions.cs   — DI registration
├── Timeout/
│   ├── ITimeoutPolicy.cs          — Interface + options
│   ├── MemoryTimeoutPolicy.cs     — Default implementation
│   └── TimeoutExtensions.cs       — DI registration
└── ResilienceExtensions.cs         — Aggregate DI registration
```

## Patterns

### Circuit Breaker
- **Purpose:** Prevent cascading failures by stopping calls to a failing dependency
- **States:** Closed → Open (after N failures) → HalfOpen (after cooldown) → Closed/Open
- **Interface:**
  - `CircuitState State { get; }`
  - `Task<T> ExecuteAsync<T>(Func<Task<T>> operation)`
  - `void Reset()`
- **Options:** `FailureThreshold`, `DurationOfBreak`, `HalfOpenMaxAttempts`

### Retry
- **Purpose:** Transient fault handling with configurable backoff
- **Backoff types:** Constant, Linear, Exponential
- **Interface:**
  - `Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken ct)`
- **Options:** `MaxRetries`, `BaseDelay`, `Backoff` (enum)

### Bulkhead
- **Purpose:** Isolate resources by limiting concurrent executions
- **Interface:**
  - `Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken ct)`
  - `int AvailableSlots { get; }`, `int QueueSize { get; }`
- **Options:** `MaxParallelization`, `MaxQueuingItems`, `QueueTimeout`

### Degradation
- **Purpose:** Graceful service degradation when dependency health declines
- **Interface:**
  - `bool IsDegraded { get; }`
  - `Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Func<T> degradedFallback)`
  - `void Reset()`
- **Options:** `DegradationThreshold`, `CheckInterval`, `DegradedConfidence`, `DegradedLevel`

### Timeout
- **Purpose:** Prevent hanging operations
- **Interface:**
  - `Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)`
- **Options:** `TimeoutDuration`

## Testing Strategy

Each pattern gets dedicated test classes:
- CircuitBreaker state machine transitions (Closed→Open, Open→HalfOpen, HalfOpen→Closed)
- Retry backoff calculation validation
- Bulkhead semaphore limit enforcement
- Degradation threshold and recovery behavior
- Timeout CancellationToken propagation
- Thread safety for concurrent access

## Acceptance Criteria

1. All 5 patterns have interface + implementation + DI registration
2. All state transitions are tested
3. All patterns are thread-safe
4. Build produces 0 warnings, 0 errors
5. All existing tests still pass

## Files

### New Source Files (~20 files)
- `src/Intentum.Runtime/Resilience/CircuitBreaker/ICircuitBreaker.cs`
- `src/Intentum.Runtime/Resilience/CircuitBreaker/MemoryCircuitBreaker.cs`
- `src/Intentum.Runtime/Resilience/CircuitBreaker/CircuitBreakerExtensions.cs`
- `src/Intentum.Runtime/Resilience/Retry/IRetryPolicy.cs`
- `src/Intentum.Runtime/Resilience/Retry/MemoryRetryPolicy.cs`
- `src/Intentum.Runtime/Resilience/Retry/RetryExtensions.cs`
- `src/Intentum.Runtime/Resilience/Bulkhead/IBulkhead.cs`
- `src/Intentum.Runtime/Resilience/Bulkhead/MemoryBulkhead.cs`
- `src/Intentum.Runtime/Resilience/Bulkhead/BulkheadExtensions.cs`
- `src/Intentum.Runtime/Resilience/Degradation/IDegradationPolicy.cs`
- `src/Intentum.Runtime/Resilience/Degradation/MemoryDegradationPolicy.cs`
- `src/Intentum.Runtime/Resilience/Degradation/DegradationExtensions.cs`
- `src/Intentum.Runtime/Resilience/Timeout/ITimeoutPolicy.cs`
- `src/Intentum.Runtime/Resilience/Timeout/MemoryTimeoutPolicy.cs`
- `src/Intentum.Runtime/Resilience/Timeout/TimeoutExtensions.cs`
- `src/Intentum.Runtime/Resilience/ResilienceExtensions.cs`

### New Test Files (~5 files)
- `tests/Intentum.Tests/Resilience/CircuitBreakerTests.cs`
- `tests/Intentum.Tests/Resilience/RetryPolicyTests.cs`
- `tests/Intentum.Tests/Resilience/BulkheadTests.cs`
- `tests/Intentum.Tests/Resilience/DegradationPolicyTests.cs`
- `tests/Intentum.Tests/Resilience/TimeoutPolicyTests.cs`
