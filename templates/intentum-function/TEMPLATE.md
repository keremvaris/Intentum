# Intentum Azure Function Template

An Azure Function with HTTP trigger for intent inference.

## How to run

```bash
dotnet new intentum-function -n MyFunction
cd MyFunction
func start
```

## Endpoints

- `POST /api/infer` — Infer intent from behavior events

## Deployment

```bash
func azure functionapp publish <app-name>
```
