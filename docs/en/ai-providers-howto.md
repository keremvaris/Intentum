# How to use AI providers

**Easy**, **medium**, and **hard** usage scenarios for each Intentum AI provider. Same structure as the usage scenarios: **What**, **What you need**, **Code**, **Expected result**.

---

## Levels

| Level | What you need | Scenario |
|-------|----------------|----------|
| **Easy** | No API key | Local run, tests, demos — Mock provider. |
| **Medium** | One API key + env vars | Single provider in console or simple app. |
| **Hard** | DI, caching, or multiple providers | ASP.NET Core, embedding cache, fallback. |

---

## Mock (Intentum.AI) — no API key

### Easy scenario: Local test or demo

**What:** You run intent inference without calling a real API; for tests, demos, or development.

**What you need:** No API key. Only the `Intentum.AI` (Mock) package.

**Code:**

```csharp
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var provider = new MockEmbeddingProvider();
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "submit");
var intent = model.Infer(space);
```

**Expected result:** `intent.Confidence.Level`, `intent.Confidence.Score`, and `intent.Signals` are populated; no real network call.

---

### Medium scenario: Mock in a single app

**What:** You use the same Mock in one console or simple app; `OPENAI_API_KEY` (etc.) is not set or you are in test mode.

**What you need:** Same as easy; Mock has no configuration.

**Code:** Same as the easy scenario.

**Expected result:** Intent inference in tests or CI without any key.

---

### Hard scenario: Swap provider via DI

**What:** You start with Mock and want to switch to a real provider (OpenAI, Gemini, etc.) in production via DI.

**What you need:** A DI container; env vars for the real provider.

**Code idea:** `LlmIntentModel(provider, similarityEngine)` and `Infer(space)` stay the same; only change the `IIntentEmbeddingProvider` registration from Mock to the real provider. See the medium scenarios below (OpenAI, Gemini, etc.) and [Providers](providers.md).

**Expected result:** Same code, different provider.

---

## OpenAI (Intentum.AI.OpenAI)

**Env vars:** `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL` (optional), `OPENAI_BASE_URL` (e.g. `https://api.openai.com/v1/`).

**Error handling:** On HTTP errors (e.g. 401, 429, 500) the provider throws. There is no built-in retry or timeout; configure `HttpClient.Timeout` and use Polly or a custom handler for retry/rate-limit. See [Embedding API error handling](embedding-api-errors.md).

### Easy scenario

**What:** You want to try without an API key.

**What you need:** OpenAI has no key-free option; use Mock instead (Mock easy scenario above).

**Expected result:** Same flow as Mock.

---

### Medium scenario: Console or minimal app

**What:** You run intent inference with OpenAI embeddings in a single console app or simple service.

**What you need:** `OPENAI_API_KEY` and `OPENAI_BASE_URL` environment variables.

**Code:**

```csharp
using Intentum.AI.OpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

// Set OPENAI_API_KEY=sk-... and OPENAI_BASE_URL=https://api.openai.com/v1/
var options = OpenAIOptions.FromEnvironment();
options.Validate();

var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.BaseUrl!)
};
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);

var provider = new OpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace()
    .Observe("user", "login")
    .Observe("user", "submit");
var intent = model.Infer(space);
```

**Expected result:** Real OpenAI embedding call; `intent` is populated.

---

### Hard scenario: ASP.NET Core + DI + cache

**What:** You use OpenAI in ASP.NET Core and want optional embedding cache to reduce cost/latency.

**What you need:** DI (`AddIntentumOpenAI`); optional `CachedEmbeddingProvider` + `MemoryEmbeddingCache` ([Advanced Features](advanced-features.md) — caching).

**Code idea:** `AddIntentumOpenAI(options)` from [Providers](providers.md); wrap the registered `IIntentEmbeddingProvider` with the cache; register `LlmIntentModel(provider, similarityEngine)` and use `Infer(space)`.

**Expected result:** Repeated embedding calls for the same behavior keys come from cache.

---

## Gemini (Intentum.AI.Gemini)

**Env vars:** `GEMINI_API_KEY`, `GEMINI_EMBEDDING_MODEL` (optional), `GEMINI_BASE_URL` (e.g. `https://generativelanguage.googleapis.com/v1beta/`).

### Easy scenario

**What:** Try without a key.

**What you need:** Use Mock (Mock easy scenario).

**Expected result:** Same flow as Mock.

---

### Medium scenario: One-off app

**What:** You run intent inference with Google Gemini embeddings in a console or simple app.

**What you need:** `GEMINI_API_KEY` and `GEMINI_BASE_URL` (or default).

**Code:**

```csharp
using Intentum.AI.Gemini;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var options = GeminiOptions.FromEnvironment();
var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.BaseUrl!)
};
var provider = new GeminiEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Expected result:** Gemini embedding call; `intent` is populated.

---

### Hard scenario: DI + cache

**What:** Use Gemini in ASP.NET Core with optional cache.

**What you need:** `AddIntentumGemini(options)`; optional `CachedEmbeddingProvider` + `MemoryEmbeddingCache`.

**Code idea:** Same pattern as OpenAI hard scenario; provider is Gemini.

**Expected result:** Fewer repeated calls with cache.

---

## Mistral (Intentum.AI.Mistral)

**Env vars:** `MISTRAL_API_KEY`, `MISTRAL_EMBEDDING_MODEL` (optional), `MISTRAL_BASE_URL` (e.g. `https://api.mistral.ai/v1/`).

### Easy scenario

**What:** Try without a key.

**What you need:** Use Mock.

**Expected result:** Same flow as Mock.

---

### Medium scenario: Single-provider app

**What:** You run intent inference with Mistral embeddings in a console or simple app.

**What you need:** `MISTRAL_API_KEY` and `MISTRAL_BASE_URL` (or default).

**Code:**

```csharp
using Intentum.AI.Mistral;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var options = MistralOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl!) };
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);

var provider = new MistralEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Expected result:** Mistral embedding call; `intent` is populated.

---

### Hard scenario: DI + cache

**What:** Use Mistral in ASP.NET Core with optional cache.

**What you need:** `services.AddIntentumMistral(options)`; optional cache (Advanced Features).

**Expected result:** Same pattern as OpenAI/Gemini hard scenario.

---

## Azure OpenAI (Intentum.AI.AzureOpenAI)

**Env vars:** `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_EMBEDDING_DEPLOYMENT` (optional), `AZURE_OPENAI_API_VERSION` (optional).

### Easy scenario

**What:** Try without a key.

**What you need:** Use Mock.

**Expected result:** Same flow as Mock.

---

### Medium scenario: Single app with Azure

**What:** You run intent inference with an Azure OpenAI embedding deployment in a console or simple app.

**What you need:** `AZURE_OPENAI_ENDPOINT` and `AZURE_OPENAI_API_KEY`.

**Code:**

```csharp
using Intentum.AI.AzureOpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

var options = AzureOpenAIOptions.FromEnvironment();
var httpClient = new HttpClient();
var provider = new AzureOpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Expected result:** Azure OpenAI embedding call; `intent` is populated.

---

### Hard scenario: DI + cache

**What:** Use Azure OpenAI in ASP.NET Core with optional cache.

**What you need:** `services.AddIntentumAzureOpenAI(options)`; optional cache.

**Expected result:** Same pattern as other providers.

---

## Claude (Intentum.AI.Claude)

**Env vars:** `CLAUDE_API_KEY`, `CLAUDE_MODEL`, `CLAUDE_BASE_URL`, `CLAUDE_API_VERSION`, `CLAUDE_USE_MESSAGES_SCORING` (optional). Claude supports both embedding and **message-based intent scoring** (ClaudeMessageIntentModel).

### Easy scenario

**What:** Try without a key.

**What you need:** Use Mock.

**Expected result:** Same flow as Mock.

---

### Medium scenario: Claude with DI, using IIntentModel

**What:** You run intent inference with Claude in a console or simple app; you register with DI and resolve `IIntentModel` (embedding or message-based scoring).

**What you need:** `CLAUDE_*` env vars; DI with `AddIntentumClaude(options)`.

**Code:**

```csharp
using Intentum.AI.Claude;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

var options = ClaudeOptions.FromEnvironment();
var services = new ServiceCollection();
services.AddIntentumClaude(options);
var sp = services.BuildServiceProvider();
var model = sp.GetRequiredService<IIntentModel>();
var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**Expected result:** Intent from Claude (embedding or message-based via `CLAUDE_USE_MESSAGES_SCORING`); `intent` is populated. See [Providers](providers.md) for exact types.

---

### Hard scenario: DI + cache

**What:** Use Claude in ASP.NET Core with optional cache; message or embedding mode.

**What you need:** `AddIntentumClaude(options)`; optional cache; model choice per package API.

**Expected result:** Same pattern as other providers; ClaudeMessageIntentModel or ClaudeIntentModel.

---

## Summary table

| Provider | Easy | Medium | Hard |
|----------|------|--------|------|
| **Mock** | MockEmbeddingProvider, no key | Same | Swap provider in DI |
| **OpenAI** | Use Mock | FromEnvironment + HttpClient + LlmIntentModel | DI + optional cache |
| **Gemini** | Use Mock | FromEnvironment + HttpClient + LlmIntentModel | DI + optional cache |
| **Mistral** | Use Mock | FromEnvironment + HttpClient + LlmIntentModel | DI + optional cache |
| **Azure OpenAI** | Use Mock | FromEnvironment + HttpClient + LlmIntentModel | DI + optional cache |
| **Claude** | Use Mock | AddIntentumClaude + IIntentModel | DI + optional cache |

---

## Security

- Never commit API keys; use environment variables or a secret manager.
- Avoid logging raw request/response bodies in production.
- For full env var names and optional fields, see each provider’s `*Options` class and `FromEnvironment()` in the repo. See also [Providers](providers.md) and [Setup](setup.md).
