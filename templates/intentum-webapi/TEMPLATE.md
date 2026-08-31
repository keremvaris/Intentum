# Intentum Web API Template

A minimal Web API with intent inference, JWT authentication, and health checks.

## How to run

```bash
dotnet new intentum-webapi -n MyApi
cd MyApi
dotnet run
```

## Endpoints

- `POST /api/intent/infer` — Infer intent from behavior events
- `GET /health` — Health check
- `GET /` — API info

## Customization

- Replace `MockEmbeddingProvider` with a real provider (OpenAI, Gemini, etc.)
- Add persistence with Intentum.Persistence.EntityFramework
- Add analytics with Intentum.Analytics
