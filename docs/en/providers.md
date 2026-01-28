# Providers (EN)

Intentum uses **embedding providers** to turn behavior (e.g. `"user:login"`) into vectors so the intent model can infer confidence. You can use a **mock provider** (no API key, for local runs and tests) or a **real provider** (OpenAI, Gemini, Mistral, Azure OpenAI, Claude) for production.

This page explains each provider: what it does, env vars, minimal code, and DI setup. For the overall flow (Observe → Infer → Decide), see [API Reference](api.md) and [Setup](setup.md).

---

## When to use which provider

| Provider | Use when |
|----------|----------|
| **MockEmbeddingProvider** (Intentum.AI) | Local runs, tests, demos — no API key. |
| **OpenAI** | You already use OpenAI; good model choice and docs. |
| **Gemini** | You prefer Google; often good latency and pricing. |
| **Mistral** | You want a European option or Mistral models. |
| **Azure OpenAI** | You run on Azure or need enterprise SLAs. |
| **Claude** | You use Anthropic; supports message-based intent scoring. |

You only need **one** embedding provider per app; pick the one that matches your stack and region. All use the same flow: `LlmIntentModel(embeddingProvider, similarityEngine)` then `Infer(space)`.

---

## OpenAI

**What it does:** Calls OpenAI’s embedding API (e.g. `text-embedding-3-large`) to turn behavior keys into vectors.

**Env vars:** `OPENAI_API_KEY`, `OPENAI_EMBEDDING_MODEL`, `OPENAI_BASE_URL` (optional, e.g. for proxy).

**Minimal code (no DI):**

```csharp
using Intentum.AI.OpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = OpenAIOptions.FromEnvironment();
options.Validate();

var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.BaseUrl ?? "https://api.openai.com/v1/")
};
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);

var provider = new OpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());

var space = new BehaviorSpace().Observe("user", "login").Observe("user", "submit");
var intent = model.Infer(space);
```

**DI (e.g. ASP.NET Core):**

```csharp
using Intentum.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;

var options = OpenAIOptions.FromEnvironment();
services.AddIntentumOpenAI(options);

// Then inject IIntentEmbeddingProvider (or OpenAIEmbeddingProvider) and build LlmIntentModel.
```

---

## Gemini

**What it does:** Calls Google’s Gemini embedding API (e.g. `text-embedding-004`) to turn behavior keys into vectors.

**Env vars:** `GEMINI_API_KEY`, `GEMINI_EMBEDDING_MODEL`, `GEMINI_BASE_URL` (optional).

**Minimal code (no DI):**

```csharp
using Intentum.AI.Gemini;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = GeminiOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl ?? "https://generativelanguage.googleapis.com/v1beta/") };
var provider = new GeminiEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());
```

**DI:**

```csharp
using Intentum.AI.Gemini;
using Microsoft.Extensions.DependencyInjection;

var options = GeminiOptions.FromEnvironment();
services.AddIntentumGemini(options);
```

---

## Mistral

**What it does:** Calls Mistral’s embedding API (e.g. `mistral-embed`) to turn behavior keys into vectors.

**Env vars:** `MISTRAL_API_KEY`, `MISTRAL_EMBEDDING_MODEL`, `MISTRAL_BASE_URL` (optional).

**Minimal code (no DI):**

```csharp
using Intentum.AI.Mistral;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = MistralOptions.FromEnvironment();
var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl ?? "https://api.mistral.ai/v1/") };
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
var provider = new MistralEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());
```

**DI:**

```csharp
using Intentum.AI.Mistral;
using Microsoft.Extensions.DependencyInjection;

var options = MistralOptions.FromEnvironment();
services.AddIntentumMistral(options);
```

---

## Azure OpenAI

**What it does:** Calls your Azure OpenAI embedding deployment to turn behavior keys into vectors. Uses endpoint + API key + deployment name.

**Env vars:** `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`, `AZURE_OPENAI_API_VERSION` (optional).

**Minimal code (no DI):**

```csharp
using Intentum.AI.AzureOpenAI;
using Intentum.AI.Models;
using Intentum.AI.Similarity;

var options = AzureOpenAIOptions.FromEnvironment();
var httpClient = new HttpClient();
// Azure uses ApiKey in header; extension usually sets it.
var provider = new AzureOpenAIEmbeddingProvider(options, httpClient);
var model = new LlmIntentModel(provider, new SimpleAverageSimilarityEngine());
```

**DI:**

```csharp
using Intentum.AI.AzureOpenAI;
using Microsoft.Extensions.DependencyInjection;

var options = AzureOpenAIOptions.FromEnvironment();
services.AddIntentumAzureOpenAI(options);
```

---

## Claude

**What it does:** Anthropic’s Claude can be used for **message-based intent scoring** (ClaudeMessageIntentModel), not only embeddings. By default the Claude package may use a stub for embeddings; use message scoring for full intent inference.

**Env vars:** `CLAUDE_API_KEY`, `CLAUDE_MODEL`, `CLAUDE_BASE_URL`, `CLAUDE_API_VERSION`, `CLAUDE_USE_MESSAGES_SCORING` (optional).

**DI (recommended):**

```csharp
using Intentum.AI.Claude;
using Microsoft.Extensions.DependencyInjection;

var options = ClaudeOptions.FromEnvironment();
services.AddIntentumClaude(options);
```

Then inject the intent model (e.g. ClaudeMessageIntentModel) as needed. See package docs for message-based scoring usage.

---

## Security and configuration

- **Never commit API keys.** Use environment variables or a secret manager.
- **Avoid logging** raw request/response bodies in production.
- **Region and latency:** Choose a provider and endpoint close to your users if latency matters.
- **Rate limits:** Respect each provider’s limits; consider retries and backoff (or a dedicated middleware) for production.

For full env var names and optional fields, see each provider’s `*Options` class and `FromEnvironment()` in the repo.
