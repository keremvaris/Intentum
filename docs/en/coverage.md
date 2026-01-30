# Coverage (EN)

Intentum generates **code coverage** for the test project so you can see which code paths are exercised by unit and contract tests. The project targets **at least 80%** line coverage on the library code; coverage is enforced via the test project’s Coverlet threshold and SonarCloud quality gate.

This page explains how to generate coverage locally, how CI generates it, and where to view the report. For what is tested, see [Testing](testing.md).

---

## Current status

- **Target: 80%+** line coverage on library code. The test project sets `Threshold=80` (Coverlet); SonarCloud quality gate can require the same for “Coverage on New Code.”
- **Covered:** Core libraries (Intentum.Core, Intentum.Runtime, Intentum.AI), provider IntentModels (OpenAI, Gemini, Mistral, Azure) with mock embedding provider, policy and clustering, options validation, and provider HTTP parsing with mock HTTP.
- **CI:** `ci.yml` runs tests with `--collect:"XPlat Code Coverage;Format=opencover"`, uploads the report for SonarCloud. `pages.yml` can publish an HTML report to GitHub Pages.

---

## Generate coverage locally

From the repository root, run tests with coverage (OpenCover format for SonarCloud):

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj \
  --collect:"XPlat Code Coverage;Format=opencover" \
  --results-directory TestResults
```

The coverage file is written under `TestResults/<run-id>/coverage.opencover.xml`.

Optional: generate an HTML report with ReportGenerator:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:TestResults/**/coverage.opencover.xml -targetdir:coverage -reporttypes:Html
```

Then open `coverage/index.html` in a browser.

---

## View the latest report (CI)

- **README badge:** The coverage badge in the README comes from **SonarCloud** (updated after each CI analysis). Click it to open the SonarCloud project summary.
- **GitHub Pages:** If `pages.yml` has run, an HTML report may be at `https://<your-org>.github.io/Intentum/coverage/index.html` (path depends on the Pages workflow).

---

## SonarCloud: findings and quality gate

- **Where to see results:** After CI runs, open [SonarCloud](https://sonarcloud.io) and select the Intentum project. The README badges (Coverage, SonarCloud alert status) link to the project summary.
- **Quality gate:** SonarCloud evaluates "Coverage on New Code", "Duplications", "Maintainability", "Reliability", "Security". The **alert status** badge is green when the quality gate passes. Fix new issues (bugs, vulnerabilities, code smells) so the gate stays green.
- **Coverage on New Code:** Only *new* code is required to meet the 80% line coverage target in the gate. Existing code is reported but does not fail the gate. Excluded paths (see below) are not counted.
- **Finding and fixing issues:** In SonarCloud, open "Issues" to see bugs, vulnerabilities, and code smells. Address new issues before merging; use "Why is this an issue?" for guidance. Common fixes: use `await` for async, avoid redundant conditions, prefer constants for repeated literals, add null checks where required.

---

## Notes

- **SonarCloud exclusions:** CodeGen (CLI tool), `*ServiceCollectionExtensions`, `*CachingExtensions`, `MultiTenancyExtensions`, and optional provider (Claude) are excluded from coverage in SonarCloud so “Coverage on New Code” reflects the tested library. See `.sonarcloud.properties`.
- **Thresholds:** The test project (`Intentum.Tests.csproj`) sets `Threshold=80` and `ThresholdType=line` so coverage is enforced; SonarCloud quality gate can also require 80% for new code.
- **Excluding code:** To exclude types or methods from Coverlet (e.g. generated code), use Coverlet’s exclude options or attributes in the project file.

For test structure and what is covered, see [Testing](testing.md).
