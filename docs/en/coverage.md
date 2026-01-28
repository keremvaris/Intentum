# Coverage (EN)

Intentum generates **code coverage** for the test project so you can see which code paths are exercised by unit and contract tests. Coverage is not 100%; the focus is on core behavior and provider contracts.

This page explains how to generate coverage locally, how CI generates it, and where to view the report. For what is tested, see [Testing](testing.md).

---

## Current status

- **Coverage is not 100%.** The project prioritizes contract tests and main behavior paths (BehaviorSpace, Infer, Decide, provider parsing).
- **Covered:** Core libraries (Intentum.Core, Intentum.Runtime, Intentum.AI) and provider parsing with mock HTTP. Some edge paths or optional features may be uncovered.
- **CI:** The GitHub Actions workflow (e.g. `pages.yml` or `ci.yml`) can run tests with coverage and publish an HTML report to GitHub Pages (e.g. `/coverage/index.html`).

---

## Generate coverage locally

From the repository root, run tests with Coverlet and OpenCover format:

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=TestResults/coverage/
```

Optional: generate an HTML report with ReportGenerator so you can browse line-by-line coverage:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:TestResults/coverage/coverage.opencover.xml -targetdir:coverage -reporttypes:Html
```

Then open `coverage/index.html` in a browser.

---

## View the latest report (CI)

If your workflow publishes coverage to GitHub Pages:

- **HTML report:** `https://<your-org>.github.io/Intentum/coverage/index.html` (or the path configured in your Pages workflow).
- **Badges:** Some workflows add a coverage badge (e.g. line coverage %) to the README; the badge links to the coverage report.

Check your `pages.yml` (or similar) for the exact path and artifact layout.

---

## Notes

- **ReportGenerator** is optional for local use; CI may already use it to produce the published HTML and badges.
- **Thresholds:** You can add a coverage threshold (e.g. fail the build if line coverage drops below X%) in your test or CI step; this repo does not enforce one by default.
- **Excluding code:** To exclude types or methods from coverage (e.g. generated code), use Coverlet’s exclude options or attributes in the project file.

For test structure and what is covered, see [Testing](testing.md).
