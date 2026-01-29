# Changelog

Bu dosya **conventional commit** mesajlarından otomatik üretilir.

| İşaret | Anlam |
|--------|--------|
| **[sürüm]** (örn. 0.0.2) | O sürümde yayınlanan değişiklikler. |
| **Breaking changes** | Geriye dönük uyumsuz değişiklik; kütüphane kullananlar kodu güncellemeli. Major sürüm (1.0.0) atlanır. |
| **Features** | Yeni özellik (feat:) → minor sürüm artar. |
| **Bug Fixes** | Hata düzeltme (fix:) → patch artar. |
| **Documentation** | Doküman (docs:). |
| **Miscellaneous** | CI, chore, bakım işleri. |

---

## [unreleased]








### Bug Fixes

- Clarify PRNG usage in BehaviorSpaceSimulator *(simulation)*



### Documentation

- Add advanced features documentation in English and Turkish *(core)*



### Miscellaneous

- Refactor token passing in CI *(sonarcloud)*

- Update GitHub Actions checkout and git-cliff-action versions *(ci)*

- Update SonarCloud coverage exclusions *(ci)*



### Refactor

- Use static evaluators and policy engines *(core)*

- Streamline code and enhance consistency *(internal)*

- Require explicit base URL for AI providers *(config)*

- Introduce RateLimitOptions for rate limit parameters *(rate-limiter)*

- Replace random generator with deterministic hashing for events *(simulation)*



### Styling

- Clean up whitespace and formatting in test files *(tests)*



### Testing

- Adds explicit base URLs to AI provider tests *(providers)*

- Update policy decision assertion to include Warn *(testing-utilities)*

- Add extensive unit tests and coverage threshold *(core)*

- Add enum value containment assertion *(PolicyDecision)*

## [0.0.7] - 2026-01-28



### Documentation

- Formalize governance and contribution *(project)*

## [0.0.6] - 2026-01-28



### Features

- Introduce multi-language support and search *(docs)*

## [0.0.5] - 2026-01-28



### Features

- Introduce CodeGen tool, web sample, and SonarCloud CI

## [0.0.4] - 2026-01-28


### Breaking changes (geriye dönük uyumsuz)

- Enhance breaking change representation *(changelog)*


## [0.0.3] - 2026-01-28



### Miscellaneous

- Enhance NuGet push robustness and API key validation *(nuget-release)*

## [0.0.2] - 2026-01-28






### Documentation

- Add documentation redirects and logo styles *(pages)*



### Features

- Integrate real AI providers, docs, and enhanced CI *(release)*

- Automate versioning, changelog, and GitHub releases *(release)*

- Automate versioning and tagging *(release)*



### Miscellaneous

- Improve repository hygiene and project configuration *(repo)*

- Compacts dotnet test commands *(workflows)*

- Improves coverage output and report generation *(coverage)*

- Improves coverage output and report generation *(coverage)*

- Improve coverage report generation *(coverage)*

- Maps source paths for accurate reports *(coverage)*

- Maps source paths for accurate reports *(coverage)*

- Remove hardcoded coverage source path *(coverage)*

- Add note for GitHub Pages source setting *(pages)*



### Refactor

- Modernize C# syntax and refine code coverage *(codebase)*

