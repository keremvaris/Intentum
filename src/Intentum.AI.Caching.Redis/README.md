# Intentum.AI.Caching.Redis

Redis-backed distributed embedding cache for Intentum. Implements `IEmbeddingCache` using StackExchange.Redis (via `IDistributedCache`) for production multi-node scenarios.

## Usage

```csharp
builder.Services.AddIntentumRedisCache(options =>
{
    options.ConnectionString = "localhost:6379";
    options.DefaultExpiration = TimeSpan.FromHours(24);
});
```

Then inject `IEmbeddingCache`; it will use Redis for storing and retrieving embeddings. Use `MemoryEmbeddingCache` for single-node or development.

## Requirements

- Redis server (local or managed, e.g. Azure Cache for Redis)
- `Intentum.AI` package

## Options

- **ConnectionString** — Redis connection string (default `localhost:6379`)
- **InstanceName** — Key prefix (default `Intentum:`)
- **DefaultExpiration** — TTL for cached embeddings (default 24 hours)
