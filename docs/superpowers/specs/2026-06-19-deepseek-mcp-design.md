# DeepSeek Provider & MCP Server Design Spec

## Goal

Add DeepSeek AI provider support and an MCP (Model Context Protocol) server to Intentum.

## Architecture

Two independent components:

### DeepSeek AI Provider
Follows the exact same pattern as `Intentum.AI.OpenAI`: options class, embedding provider (OpenAI-compatible API), thin intent model subclass, DI extension, rate limit exception. DeepSeek uses OpenAI-compatible API format, so `DeepSeekEmbeddingProvider` calls `https://api.deepseek.com/v1/embeddings`.

### MCP Server
ASP.NET minimal API that exposes Intentum functionality as MCP tools. Any MCP-compatible AI client (Claude, Cursor, Copilot, DeepSeek) can discover and call these tools.

## Components

### DeepSeek Provider

| File | Purpose |
|------|---------|
| `DeepSeekOptions.cs` | `ApiKey`, `BaseUrl` (default `https://api.deepseek.com/v1`), `EmbeddingModel` (default `deepseek-embedding`), `FromEnvironment()` |
| `DeepSeekEmbeddingProvider.cs` | `IIntentEmbeddingProvider`: HTTP POST to `/embeddings` with Bearer auth, 429 retry via `EmbeddingHttpRetryHandler` |
| `DeepSeekIntentModel.cs` | `: ProviderLlmIntentModelBase("deepseek", ...)` — 10 line subclass |
| `DeepSeekServiceCollectionExtensions.cs` | `AddIntentumDeepSeek(DeepSeekOptions)` |
| `DeepSeekRateLimitException.cs` | Custom 429 exception |

### MCP Server

| Tool | Description | Input | Output |
|------|-------------|-------|--------|
| `infer_intent` | Analyze behavior events → intent | `events[]` | `{name, confidence}` |
| `evaluate_policy` | Evaluate intent against policy rules | `{intent, rules[]}` | `{decision}` |
| `detect_anomaly` | Detect anomalous patterns | `{events[], threshold}` | `{anomaly, score}` |

## Acceptance Criteria

1. DeepSeek provider builds with 0 errors
2. DeepSeek provider works with real DeepSeek API (env var based)
3. MCP server starts and exposes tools
4. MCP tools accept JSON input and return structured output

## Files

### DeepSeek Provider (5 files)
- `src/Intentum.AI.DeepSeek/Intentum.AI.DeepSeek.csproj`
- `src/Intentum.AI.DeepSeek/DeepSeekOptions.cs`
- `src/Intentum.AI.DeepSeek/DeepSeekEmbeddingProvider.cs`
- `src/Intentum.AI.DeepSeek/DeepSeekIntentModel.cs`
- `src/Intentum.AI.DeepSeek/DeepSeekServiceCollectionExtensions.cs`

### MCP Server (4+ files)
- `src/Intentum.McpServer/Intentum.McpServer.csproj`
- `src/Intentum.McpServer/Program.cs`
- `src/Intentum.McpServer/McpTools/InferIntentTool.cs`
- `src/Intentum.McpServer/McpTools/EvaluatePolicyTool.cs`
