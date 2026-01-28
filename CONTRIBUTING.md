# Contributing to Intentum

Thank you for considering contributing to Intentum. This document explains how to get started and what we expect.

## How to contribute

- **Bug reports & feature requests** — Open an [issue](https://github.com/keremvaris/Intentum/issues/new/choose) and choose a template: **Bug report**, **Feature request**, **Documentation**, or **Question**. Describe the problem or idea and, for bugs, steps to reproduce.
- **Code & documentation** — Open a pull request (PR) against `master`. Keep changes focused; larger work can be split into multiple PRs.
- **Documentation** — Docs live in `docs/en/` (English) and `docs/tr/` (Turkish). Please update both when adding or changing user-facing docs. Sidebar structure is in `docs/toc.yml`, `docs/en/toc.yml`, and `docs/tr/toc.yml`.

## Development setup

1. **Clone** (or fork and clone):
   ```bash
   git clone https://github.com/keremvaris/Intentum.git
   cd Intentum
   ```

2. **Build & test** (requires [.NET SDK](https://dotnet.microsoft.com/download) matching the repo, e.g. .NET 10):
   ```bash
   dotnet restore Intentum.slnx
   dotnet build Intentum.slnx --no-restore
   dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --no-build
   ```

3. **Optional** — Run the sample and Sample.Web:
   ```bash
   dotnet run --project samples/Intentum.Sample
   dotnet run --project samples/Intentum.Sample.Web   # UI: http://localhost:5150/
   ```

## Pull request process

- Branch from `master`, make your changes, and open a PR to `master`.
- CI runs on every push/PR: build, tests, and SonarCloud analysis. Please ensure the build and tests pass locally and that you address any new SonarCloud issues you introduce.
- Maintainers will review and may request changes. Once approved, your PR can be merged.

## Code and style

- **C#** — Follow existing style in the repo. The project uses SonarCloud; avoid new code smells (e.g. use `await` for async, avoid redundant conditions, prefer constants for repeated literals).
- **Docs** — Use Markdown. For DocFX (docs site), keep `docs/en/` and `docs/tr/` in sync when changing content.
- **CodeGen** — If you change the CodeGen tool or templates under `src/Intentum.CodeGen/` or `templates/intentum-cqrs/`, run a quick scaffold/generate cycle and update [docs/en/codegen.md](docs/en/codegen.md) and [docs/tr/codegen.md](docs/tr/codegen.md) if behavior or usage changes.

## License and conduct

- By contributing, you agree that your contributions will be licensed under the same terms as the project ([MIT License](LICENSE)).
- Please follow our [Code of Conduct](CODE_OF_CONDUCT.md).

If you have questions, open an issue and we’ll help.
