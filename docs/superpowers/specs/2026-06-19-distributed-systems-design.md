# Distributed Systems Design Spec

## Goal

Add 5 distributed system capabilities (Distributed Locking, Distributed Rate Limiter, Event Sourcing, Outbox Pattern, gRPC) for multi-instance deployment scenarios.

## Architecture

```
src/Intentum.Distributed/                     — Interfaces only (0 external deps)
├── Locking/IDistributedLock.cs
├── RateLimiting/IDistributedRateLimiter.cs
├── EventSourcing/IAggregateRoot.cs, IEventStore.cs, IEventBus.cs, IDomainEvent.cs
└── Outbox/IOutboxStore.cs, IOutboxProcessor.cs, OutboxMessage.cs

src/Intentum.Distributed.Redis/               — Redis implementations
├── RedisDistributedLock.cs
├── RedisDistributedRateLimiter.cs
└── DistributedRedisExtensions.cs

src/Intentum.Grpc/                            — gRPC service definitions
├── Protos/intentum.proto
└── Services/IntentumGrpcService.cs
```

## Patterns

### Distributed Locking
- `IDistributedLock` — `AcquireAsync(key, timeout)`, `ReleaseAsync()`
- `RedisDistributedLock` — StackExchange.Redis `LockTake`/`LockRelease`
- Default lock timeout per key, auto-release on dispose

### Distributed Rate Limiter
- `IDistributedRateLimiter` — `TryAcquireAsync(key, limit, window)`
- `RedisDistributedRateLimiter` — INCR + EXPIRE with sliding window
- Follows same pattern as in-memory `IRateLimiter`

### Event Sourcing
- `IDomainEvent` — marker interface
- `IAggregateRoot` — `Id`, `Events`, `ClearEvents()`
- `IEventStore` — `AppendAsync(streamId, events)`, `ReadAsync(streamId, fromVersion)`
- `IEventBus` — `PublishAsync<T>(T event)`

### Outbox Pattern
- `OutboxMessage(Guid Id, string Type, string Payload, DateTime CreatedAt, bool Processed)`
- `IOutboxStore` — `SaveAsync()`, `GetUnprocessedAsync(batchSize)`, `MarkProcessedAsync(id)`
- `IOutboxProcessor` — `ProcessAsync(CancellationToken)`

### gRPC
- `intentum.proto` — Infer + Evaluate RPC definitions
- `IntentumGrpcService` — gRPC service implementation delegating to runtime

## Acceptance Criteria

1. All interfaces build with 0 dependencies beyond .NET BCL
2. Redis implementations work with StackExchange.Redis
3. gRPC service compiles with protobuf tooling
4. All existing tests still pass

## Files

### New Projects (3)
- `src/Intentum.Distributed/Intentum.Distributed.csproj`
- `src/Intentum.Distributed.Redis/Intentum.Distributed.Redis.csproj`
- `src/Intentum.Grpc/Intentum.Grpc.csproj`

### New Source Files (~12)
- `src/Intentum.Distributed/Locking/IDistributedLock.cs`
- `src/Intentum.Distributed/RateLimiting/IDistributedRateLimiter.cs`
- `src/Intentum.Distributed/EventSourcing/IDomainEvent.cs`
- `src/Intentum.Distributed/EventSourcing/IAggregateRoot.cs`
- `src/Intentum.Distributed/EventSourcing/IEventStore.cs`
- `src/Intentum.Distributed/EventSourcing/IEventBus.cs`
- `src/Intentum.Distributed/Outbox/OutboxMessage.cs`
- `src/Intentum.Distributed/Outbox/IOutboxStore.cs`
- `src/Intentum.Distributed/Outbox/IOutboxProcessor.cs`
- `src/Intentum.Distributed.Redis/RedisDistributedLock.cs`
- `src/Intentum.Distributed.Redis/RedisDistributedRateLimiter.cs`
- `src/Intentum.Distributed.Redis/DistributedRedisExtensions.cs`
- `src/Intentum.Grpc/Protos/intentum.proto`
- `src/Intentum.Grpc/Services/IntentumGrpcService.cs`
