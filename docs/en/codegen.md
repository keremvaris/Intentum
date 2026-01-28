# CodeGen (EN)

Intentum CodeGen scaffolds CQRS + Intentum projects and generates **Features** (Commands, Queries, Handlers, Validators) from a test assembly or a YAML/JSON spec. Use it in **any** solution: new project or existing Web API.

---

## Quick start (full usage)

1. **Scaffold** a CQRS + Intentum project (new or into an existing folder):
   ```bash
   dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- scaffold -o ./MyApp
   cd MyApp && dotnet build
   ```
2. **Optional — generate features** from your test assembly or a YAML spec:
   ```bash
   dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- generate -a ./tests/MyApp.Tests/bin/Debug/net10.0/MyApp.Tests.dll -o ./MyApp
   # or: -- generate -s features.yaml -o ./MyApp
   ```
3. **Run** the app (default port may be 5000 or 5150; see `Properties/launchSettings.json`). Open the **UI** at `http://localhost:5150/` and **API docs** at `http://localhost:5150/scalar`.

Existing files are never overwritten; CodeGen only adds missing files.

---

## Modes

| Mode | Input | Output |
|------|--------|--------|
| **Scaffold** | Target directory (`-o`) | Intentum + CQRS project skeleton, `Features/` folder, sample feature |
| **Generate** | Test assembly (`-a`) or spec file (`-s`) + output directory | `Features/<FeatureName>/` with Commands, Queries, Handlers, Validators |

---

## Running CodeGen

From the repo root:

```bash
# Scaffold a new project into a folder (new or existing)
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- scaffold -o ./MyApp
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- scaffold --output ./MyApp

# Generate CQRS feature code from a test assembly (xUnit [Fact] methods)
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- generate -a ./tests/MyApp.Tests/bin/Debug/net10.0/MyApp.Tests.dll -o ./src/MyApp.Web

# Generate from a YAML spec
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- generate -s features.yaml -o ./src/MyApp.Web
```

You can use this in **any project**: point `-o` at your existing Web API or class library; CodeGen writes into that folder.

---

## dotnet new template

Install the Intentum CQRS template and create a new project:

```bash
dotnet new install ./templates/intentum-cqrs
dotnet new intentum-cqrs -n MyApp -o ./MyApp
```

---

## Test assembly → Features

CodeGen scans the test assembly for methods with **xUnit `[Fact]`** (or similar) and parses **method names** by convention:

- Pattern: `FeatureName_Action_ByX` (e.g. `CarbonFootprintCalculation_AllowsOrObserves_ByConfidence`).
- **Feature name** = first segment (`CarbonFootprintCalculation`).
- For each unique feature name it generates:
  - `Features/<FeatureName>/Commands/` — one command + result record.
  - `Features/<FeatureName>/Handlers/` — command handler.
  - `Features/<FeatureName>/Validators/` — FluentValidation validator (optional).

Handlers and validators are minimal stubs; you can add Intentum (Observe → Infer → Decide) inside handlers as in [Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web).

---

## YAML spec format

Example `features.yaml`:

```yaml
namespace: MyApp.Web
features:
  - name: CarbonFootprintCalculation
    commands:
      - name: CalculateCarbon
        properties:
          - name: Actor
            type: string
          - name: Scope
            type: string
    queries:
      - name: GetCarbonReport
        properties:
          - name: ReportId
            type: string
```

- **namespace** — Root namespace for generated C# (default: `Intentum.Cqrs.Web`).
- **features** — List of features; each can have **commands** and **queries**.
- **commands** / **queries** — Name and optional **properties** (name + type). Generated code uses MediatR `IRequest<T>` and FluentValidation where applicable.

---

## Output structure

Scaffold and generate both target this layout (aligned with [Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web)):

```
Features/
  <FeatureName>/
    Commands/
    Queries/
    Handlers/
    Validators/
```

Existing files are **not overwritten**; CodeGen only writes missing files.

---

## Sample.Web at a glance

The [Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) project is the reference for CodeGen output and Intentum in handlers:

- **Stack:** ASP.NET Core, MediatR, FluentValidation, Intentum (Core, Runtime, AI), **Scalar** (API docs), static **UI** in `wwwroot/`.
- **Endpoints:** `POST /api/carbon/calculate` (with Intentum Observe→Infer→Decide), `GET /api/carbon/report/{id}`, `POST /api/orders`.
- **UI:** `http://localhost:5150/` — forms to try Carbon, Report, and Orders. **API docs:** `http://localhost:5150/scalar`.
- **Port:** 5150 in `Properties/launchSettings.json` (change if 5000 is in use).
- **Run:** `dotnet run --project samples/Intentum.Sample.Web`.

---

## See also

- [Setup](setup.md) — Install Intentum packages.
- [Scenarios](scenarios.md) — Observe → Infer → Decide examples.
- [Sample.Web](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Web) — Full CQRS + Intentum ASP.NET sample (Scalar + UI).
