# Changelog

Bu dosya **conventional commit** mesajlarından otomatik üretilir. Commit'te **açıklama (body)** veya **BREAKING CHANGE:** eklersen CHANGELOG'da detay ve geriye dönük uyumsuzluk metni görünür. Nasıl yazılır: [CONTRIBUTING.md — Commit messages and CHANGELOG](CONTRIBUTING.md#commit-messages-and-changelog).

| İşaret | Anlam |
|--------|--------|
| **[sürüm]** (örn. 0.0.2) | O sürümde yayınlanan değişiklikler. |
| **Breaking changes** | Geriye dönük uyumsuz değişiklik; kütüphane kullananlar kodu güncellemeli. **BREAKING CHANGE:** footer'ı veya `feat!:` kullan. |
| **Features** | Yeni özellik (feat:) → minor sürüm artar. |
| **Bug Fixes** | Hata düzeltme (fix:) → patch artar. |
| **Documentation** | Doküman (docs:). |
| **Miscellaneous** | CI, chore, bakım işleri. |

---

## [1.0.0] - 2026-01-30




### Features

- **Add behavior vector normalization and rule/chained intent models** *(core)*

- Introduced ToVectorOptions for BehaviorSpace.ToVector to support normalization: Cap, L1 norm, SoftCap
- Enabled optional normalization when building behavior vectors, including time-windowed vectors
- Added RuleBasedIntentModel supporting intent inference purely from rules with explainable output
- Added ChainedIntentModel that first tries a primary model and falls back to secondary based on confidence threshold
- Modified LlmIntentModel to use dimension counts as weights for similarity engine when supported
- Integrated ITimeAwareSimilarityEngine support in LlmIntentModel for automatic time decay application
- Updated documentation to cover source weights, vector normalization, rule-based and chained intent models
- Added comprehensive tests for BehaviorSpace.ToVector with normalization options





### Miscellaneous

- **Add check to sync EN and TR doc menus in workflow** *(docs)*

- Added a workflow step to compare href entries in docs/en/toc.yml and docs/tr/toc.yml
- Ensured both toc.yml files have identical hrefs in the same order
- Fails the workflow with a diff output if the href lists differ
- Helps maintain consistency between English and Turkish documentation menus



## [0.0.12] - 2026-01-29



### Documentation

- **Add detailed overview and usage of extended Intentum packages** *(advanced-features)*

- Add a comprehensive packages and features overview section for Intentum extended packages
- Document how to set up and use Redis-based embedding caching with Intentum.AI.Caching.Redis
- Introduce policy composition techniques: inheritance, merge, and A/B policy variants
- Explain rate limiting usage with IRateLimiter and MemoryRateLimiter
- Describe streaming inference methods InferMany and InferManyAsync
- Detail persistence options with Entity Framework Core, Redis, and MongoDB implementations
- Provide usage instructions for the webhook/event system with Intentum.Events
- Document intent clustering concepts, strategies, and setup using Intentum.Clustering
- Explain intent explainability using Intentum.Explainability with signal contributions and summaries
- Describe intent simulation with Intentum.Simulation for generating synthetic behavior spaces
- Present A/B experiments with Intentum.Experiments including variant setup and traffic splitting
- Detail multi-tenancy support via Intentum.MultiTenancy with tenant-scoped repositories
- Explain policy versioning capabilities with Intentum.Versioning for rollback and rollout management
- Add internal MetaPackage stub referencing core, runtime, AI, and all provider packages for full adapter inclusion



## [0.0.11] - 2026-01-29



### Refactor

- **Migrate to Intentum.Core.Intents namespace** *(intent)*

- Move Intent, IntentConfidence, and IntentSignal to Intentum.Core.Intents namespace
- Update all using directives to reference Intentum.Core.Intents instead of Intentum.Core.Intent
- Modify method signatures and return types from Intents.Intent to Intent
- Document IntentSignal in English and Turkish API docs
- Update architecture diagrams to specify Intentum.Core.Intents for IntentSignal
- Adjust nullability for parameters and return types in batch intent model methods
- Refactor codegen handlers for consistency
- Remove unused/useless using directives for cleaner code
- Update tests to align with namespace changes and nullability adjustments
- Add namespace usage instructions to documentation



## [0.0.10] - 2026-01-29



### Miscellaneous

- **Centralize README.md packaging in root props file** *(build)*

- Remove individual README.md packaging entries from all project files
- Add a single ItemGroup in Directory.Build.props to include README.md for all packages
- Prevent NU5039 warnings by packaging README.md from repo root uniformly
- Simplify project file maintenance by centralizing packaging configuration



## [0.0.9] - 2026-01-29



### Documentation

- **Update coverage documentation with threshold and CI details** *(coverage)*

- Add library line coverage target of 80% enforced via Coverlet and SonarCloud
- Expand coverage scope to include provider IntentModels, policy, clustering, and options validation
- Update local coverage generation commands to use XPlat Code Coverage and results directory options
- Clarify CI workflow coverage collection and SonarCloud upload steps in docs
- Describe exclusions for generated code and specific extensions in SonarCloud properties
- Update README badge source and GitHub Pages report location details
- Enhance testing docs with detailed areas tested and new extensions in both English and Turkish versions



## [0.0.8] - 2026-01-29








### Bug Fixes

- **Clarify PRNG usage in BehaviorSpaceSimulator** *(simulation)*

- Added comment explaining PRNG usage for synthetic data generation and determinism when seeded
- Updated MockEmbeddingProvider to use deterministic hash expansion instead of PRNG for vectors
- Added HashForKeyAndIndex method for consistent vector generation from keys
- Added assertion in TestingUtilitiesTests to ensure inferred intent is not null before confidence checks





### Documentation

- **Add advanced features documentation in English and Turkish** *(core)*

- Add detailed docs for similarity engines including simple average, weighted, time decay, cosine, and composite
- Document fluent APIs: BehaviorSpaceBuilder and IntentPolicyBuilder with usage examples
- Describe new policy decision types: Escalate, RequireAuth, RateLimit with localization support
- Explain rate limiting usage with IRateLimiter and MemoryRateLimiter examples
- Add embedding caching options: in-memory and Redis distributed cache
- Document behavior space metadata and time window analysis features
- Provide testing utilities usage with assertions and helpers
- Introduce intent analytics package with setup, confidence trends, anomaly detection, and export functions
- Document ASP.NET Core middleware for behavior observation
- Add observability package with OpenTelemetry metrics overview
- Describe batch processing with BatchIntentModel for efficient inference
- Document persistence layer integration with EF Core and intent history repositories
- Detail webhook/event system for intent events with retry mechanism
- Present intent clustering package for pattern detection in history records
- Include intent explainability package usage for signal contributions and summaries
- Add intent simulation package for synthetic behavior space generation
- Provide A/B experiment package overview for traffic splitting on variants
- Introduce multi-tenancy support for tenant-scoped repositories
- Add policy versioning package for rollback and version tracking
- Extend API docs with new fluent policy builder and decision enum entries
- Provide structured logging and health check integration examples in Turkish docs
- Add Intentum.Analytics service registration extension and analytics models for summary and anomaly reports



- **Update coverage badge to use SonarCloud** *(readme)*

- Replaced coverage badge URL in README.md to link SonarCloud coverage data
- Updated README.tr.md with the same SonarCloud coverage badge change
- Improved accuracy and reliability of coverage status display in project documentation





### Miscellaneous

- **Refactor token passing in CI** *(sonarcloud)*

Ensures the SonarCloud token is passed via an environment variable, improving secret handling within the workflow.



- **Update GitHub Actions checkout and git-cliff-action versions** *(ci)*

- Upgrade actions/checkout to commit 34e114876b0b11c390a56381ad16ebd13914f8d5
- Bump orhun/git-cliff-action to commit e16f179f0be49ecdfe63753837f20b9531642772
- Update softprops/action-gh-release to commit a06a81a03ee405af7f2048a818ed3f03bbf83c7b
- Ensure consistent usage of specific commit SHAs for workflow stability



- **Update SonarCloud coverage exclusions** *(ci)*

- Add sonar.coverage.exclusions property to .sonarcloud.properties
- Exclude CLI/tool and optional integration code not covered by unit tests
- Update GitHub Actions workflow to include coverage exclusions in SonarCloud analysis
- Ensure coverage metrics reflect tested library code without dilution from excluded files and folders





### Refactor

- **Use static evaluators and policy engines** *(core)*

Converts `IntentEvaluator` and `IntentPolicyEngine` to static classes, simplifying usage and promoting stateless design.

Extracts string literals in `Intentum.Sample` to a dedicated `SampleLiterals.cs` file, improving code readability and maintainability.

Refines GitHub Actions workflow permissions to job-specific scopes for enhanced security. Updates HTML redirects to use `globalThis` and removes deprecated `meta http-equiv="refresh"` for better compatibility.

Migrates the `release.sh` script to `bash` for improved robustness in conditional logic and adds error handling for version bumping.

Standardizes default base URLs for AI embedding providers (OpenAI, Claude, Gemini, Mistral) using constants and adds option validation for Claude, ensuring configuration consistency and reliability.



- **Streamline code and enhance consistency** *(internal)*

Removes redundant parameter names from ArgumentException messages across AI provider options.
Encapsulates the SampleLiterals class within its designated namespace.
Removes an unused field in ClaudeEmbeddingProvider for cleaner code.
Optimizes policy rule matching logic in IntentPolicyEngine.
Adjusts the internal map type in DefaultLocalizer for direct Dictionary access.



- **Require explicit base URL for AI providers** *(config)*

Removes default base URLs for OpenAI, Claude, Gemini, and Mistral options.
The BaseUrl for each AI provider must now be explicitly configured via their respective environment variables (e.g., OPENAI_BASE_URL).
Throws an InvalidOperationException if the _BASE_URL environment variable is not set or is empty.



- **Introduce RateLimitOptions for rate limit parameters** *(rate-limiter)*

- Replace separate rateLimitKey, limit, and window parameters with RateLimitOptions record
- Update DecideWithRateLimit and DecideWithRateLimitAsync methods to use RateLimitOptions
- Update documentation to include RateLimitOptions usage for rate limiting
- Simplify MemoryRateLimiter Tick method by removing unused limit parameter
- Improve constant usage for exception message in BehaviorSpaceBuilder
- Enhance error validation in SpecValidator with modularized validation methods
- Fix cosine similarity engine numerical stability by using epsilon threshold
- Refactor embedding cache to use updated FusionCache API and types
- Minor fixes and improvements in tests and samples for consistency
- Update webhooks tests with predefined event type arrays for clarity
- Correct small issues in clustering, experiments, and mock embedding provider implementations
- Update sample usage to reflect RateLimitOptions in place of individual parameters
- Adjust package references for FusionCache and Observability dependencies
- Fix example documentation hyperlinks formatting and wording



- **Replace random generator with deterministic hashing for events** *(simulation)*

- Remove use of Random class for synthetic data generation
- Introduce hashing method based on seed, index, and salt for deterministic actor and action selection
- Calculate time increments deterministically using hashing
- Ensure consistent synthetic data generation with given seed or fallback tick count
- Add private static Hash method to compute deterministic pseudo-random values
- Improve clarity and efficiency by precomputing actors and actions count





### Styling

- **Clean up whitespace and formatting in test files** *(tests)*

- Remove trailing whitespace and add blank lines for consistency in batch processing tests
- Fix spacing around braces and blank lines in similarity engine tests
- Adjust lambda parameter naming for simplicity in sample web program
- Add missing newlines in AspNetCore extensions and entity classes
- Reformat comments and spacing in CosineSimilarityEngine and TimeDecaySimilarityEngine

fix(persistence): ensure MetadataJson serialization defaults to empty object

- Add default "{}" for null metadata serialization in BehaviorSpaceEntity and IntentHistoryEntity
- Fix missing fallback colon in BehaviorEventEntity metadata serialization
- Improve null checks during metadata deserialization in BehaviorSpaceRepository

test(policy): add unit tests for PolicyDecision ToLocalizedString method

- Cover Allow, Observe, Warn, and Block policy decisions with localized string assertions
- Verify localized string outputs align with default localizer expectations

style(runtime): add spacing in PolicyDecision enum comments

- Insert blank lines between enum members and their XML summary comments for clarity





### Testing

- **Adds explicit base URLs to AI provider tests** *(providers)*

Ensures AI embedding providers (OpenAI, Gemini, Mistral) are configured with their explicit base URLs during HTTP integration tests. This improves test accuracy and configuration realism.



- **Update policy decision assertion to include Warn** *(testing-utilities)*

- Modified assertion to accept PolicyDecision.Warn in addition to Allow and Observe
- Updated comment to reflect that default policy may return Warn based on inferred intent



- **Add extensive unit tests and coverage threshold** *(core)*

- Add IntentPolicyBuilder rule addition test verifying custom rule creation
- Add BehaviorSpaceBuilder tests for action metadata and timestamp usage
- Add IntentClusterer tests covering clustering, ID, records, summary, and DI registration
- Add IntentConfidence level mapping test from score ranges
- Add BehaviorSpace evaluation test ensuring normalization of intent signal weights
- Add OpenAI service registration and inference tests with mock implementations
- Add various utility and assertion helper tests verifying model creation and policy decisions
- Introduce 80% minimum line coverage threshold in test project configuration
- Update CI workflow to enforce coverage exclusions and pass coverage reports to SonarCloud



- **Add enum value containment assertion** *(PolicyDecision)*

- Add test to verify PolicyDecision.Allow is included in the enum values
- Ensure enum completeness check is covered in unit tests



- **Add validation and inference tests for AI options and models** *(intent)*

- Add unit tests for AzureOpenAIOptions validation covering all required properties
- Add unit tests for GeminiOptions and MistralOptions validation with various empty fields
- Add inference tests for GeminiIntentModel, MistralIntentModel, and AzureOpenAIIntentModel
- Use mock embedding providers and similarity engines for intent inference tests
- Verify inferred intent properties including name, signals, and confidence scores where applicable



## [0.0.7] - 2026-01-28



### Documentation

- **Formalize governance and contribution** *(project)*

Adds `CODE_OF_CONDUCT.md`, `CONTRIBUTING.md`, `LICENSE`, and `SECURITY.md` for clear project guidelines.
Introduces dedicated issue templates (`bug_report`, `feature_request`, `documentation`, `question`) and `config.yml` to streamline issue reporting.
Updates READMEs to include links to new documentation and a SonarCloud badge.
Refactors `CalculateCarbonResponse` in the sample web app to use an interface, improving flexibility.
Enhances the code generator by adopting `GeneratedRegex` and `AssemblyLoadContext` for better performance and extensibility.
Applies minor code style and cleanup in samples and tests, including JavaScript parsing methods and policy rule formatting.



## [0.0.6] - 2026-01-28



### Features

- **Introduce multi-language support and search** *(docs)*

Configures DocFX to process `toc.yml` files for navigation.
Establishes top-level and language-specific (English, Turkish) table of contents.
Enables search functionality in the generated documentation.
Adds a note to README regarding SonarCloud automatic analysis settings.



## [0.0.5] - 2026-01-28



### Features

- **Introduce CodeGen tool, web sample, and SonarCloud CI**

Adds a new CodeGen CLI tool to scaffold CQRS + Intentum projects and generate features from test assemblies or YAML specifications. This includes a `dotnet new` template for rapid project creation.

Introduces `Intentum.Sample.Web`, a comprehensive ASP.NET Core Web API sample demonstrating CQRS with MediatR, FluentValidation, Intentum AI integration, Scalar API documentation, and a basic web UI.

Enhances the CI pipeline with SonarCloud integration for static code analysis, improving code quality and maintainability checks. Updates `.gitignore` and documentation to reflect these changes.



## [0.0.4] - 2026-01-28


### Breaking changes (geriye dönük uyumsuz)

- **Enhance breaking change representation** *(changelog)*

- **Açıklama:** Improves git-cliff configuration to explicitly handle and display breaking changes in the CHANGELOG.

Adds a detailed changelog header explaining the format and conventions. Introduces dedicated commit parsers to identify breaking changes via `!` in the commit type (e.g., `feat!:`) or `BREAKING CHANGE:` in the commit body, ensuring they appear in a dedicated section at the top of each release.

Updates README.md to guide contributors on how to properly mark breaking changes for correct changelog generation and version bumping.



## [0.0.3] - 2026-01-28



### Miscellaneous

- **Enhance NuGet push robustness and API key validation** *(nuget-release)*

Refactors the NuGet package pushing logic to use `find -exec`, improving reliability across various package scenarios. Adds a crucial check for the `NUGET_API_KEY` secret, preventing workflow failures due to a missing API key and providing a clear error message.



## [0.0.2] - 2026-01-28






### Documentation

- **Add documentation redirects and logo styles** *(pages)*

Introduces `index.html` files at the root (`/docs/`) and API (`/docs/api/`) paths to automatically redirect users to the Turkish version of the documentation (e.g., `api/tr/index.html`). This ensures a consistent entry point for documentation access.

Also, adds CSS rules to constrain the maximum height of various logo images within documentation templates, preventing them from appearing excessively large and improving overall page layout.





### Features

- **Integrate real AI providers, docs, and enhanced CI** *(release)*

This release transitions the AI embedding providers from deterministic stubs to real HTTP API calls for OpenAI, Gemini, Mistral, Azure OpenAI, and Claude. It also introduces a comprehensive documentation site (EN/TR) powered by DocFX and GitHub Pages.

Key changes include:
- **Real AI Integrations**: Implement HTTP clients and options for all major AI embedding providers, replacing mock implementations.
- **Comprehensive Documentation**: Add extensive multi-language documentation covering core concepts, API, providers, scenarios, setup, and more.
- **GitHub Pages Workflow**: Set up automated deployment of documentation and code coverage reports to GitHub Pages.
- **Enhanced CI/CD**: Update CI to include full code coverage generation and artifact upload. Improve NuGet release process with dynamic versioning and .slnx migration.
- **Expanded Test Suite**: Introduce detailed scenario tests (low, medium, high complexity, sector-specific, workflow status) and HTTP mock tests for AI providers.
- **Updated Sample Application**: Showcase a wider range of scenarios and demonstrate switching between mock and real AI providers.
- **Localization**: Add basic localization for policy decisions.
- **Build System**: Migrate to .slnx solution format and standardize build properties across projects.



- **Automate versioning, changelog, and GitHub releases** *(release)*

Integrates git-cliff for automatic generation of CHANGELOG.md and release notes based on conventional commits.
Adopts MinVer for deriving package versions directly from Git tags, removing the need for manual version updates.
Refactors GitHub Actions workflows for NuGet and GitHub Releases to leverage these new tools, enhancing automation and consistency.
Updates documentation, sample code, and tests to include new e-commerce scenarios and remove old Sukuk & Islamic Finance examples, broadening the project's showcased applicability.



- **Automate versioning and tagging** *(release)*

Introduces a `release.sh` script to automatically determine and apply the next semantic version based on conventional commits since the last tag, or allow manual version specification.

Enhances GitHub release workflows (`github-release.yml`) with `workflow_dispatch` to enable manual triggering and direct tag input. Updates `README.md` to document the new release script and manual workflow capabilities.





### Miscellaneous

- **Improve repository hygiene and project configuration** *(repo)*

Adds a comprehensive `.gitignore` to exclude build outputs, NuGet packages, and IDE/OS specific files.
Removes existing build artifacts and temporary files from version control.
Updates repository URLs in all project files (csproj).
Enhances `README.md` readability by adding markdown code block formatting and updating the CI badge link.



- **Compacts dotnet test commands** *(workflows)*


- **Improves coverage output and report generation** *(coverage)*

Corrects the relative path for `CoverletOutput` in CI/CD workflows to ensure coverage files are saved in the expected location. Updates the `reportgenerator` command to use a wildcard for input reports, making it more robust to potential filename variations.



- **Improves coverage output and report generation** *(coverage)*

Corrects the relative path for `CoverletOutput` in CI/CD workflows to ensure coverage files are saved in the expected location. Updates the `reportgenerator` command to use a wildcard for input reports, making it more robust to potential filename variations.



- **Improve coverage report generation** *(coverage)*

Simplifies coverage test command by removing explicit output path.
Adjusts report discovery to broadly find `.opencover.xml` files.
Adds a robust check for missing coverage reports, providing clearer error feedback.



- **Maps source paths for accurate reports** *(coverage)*

Resolves path discrepancies for source code links in HTML coverage reports generated in CI.
This ensures correct navigation to source files within the reports.



- **Maps source paths for accurate reports** *(coverage)*

Resolves path discrepancies for source code links in HTML coverage reports generated in CI.
This ensures correct navigation to source files within the reports.



- **Remove hardcoded coverage source path** *(coverage)*

Removes a developer-specific local path from the `sourcedirs` parameter during coverage report generation. This improves the portability and reusability of the CI workflow by preventing machine-specific configurations from being used.



- **Add note for GitHub Pages source setting** *(pages)*




### Refactor

- **Modernize C# syntax and refine code coverage** *(codebase)*

Updates projects to leverage C# 12 primary constructors for dependency injection across AI embedding providers and intent models, simplifying constructor definitions.

Restructures core `Intent` types into a dedicated `Intentum.Core.Intent` namespace for clearer organization.

Reconfigures GitHub Actions CI/CD workflows to use `XPlat Code Coverage` for test coverage collection and report generation, migrating from direct Coverlet parameters to a standard data collector. This streamlines the coverage process and ensures `cobertura.xml` output.

Adds XML documentation comments to public API members in `Intentum.Core` and suppresses `CS1591` warnings in project files for better consistency. Includes minor code modernizations such as empty collection initialization (`[]`) and simplified LINQ expressions.



