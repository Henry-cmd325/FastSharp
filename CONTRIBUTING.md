# Contributing to FastSharp

Thank you for your interest in contributing to FastSharp. This guide explains how we work on the repository so changes stay consistent, reviewable, and easy to integrate.

All contributions — code, documentation, tests, and samples — should follow the workflow and conventions below.

---

## Branching model

FastSharp uses two long-lived branches:

| Branch | Purpose |
|--------|---------|
| `main` | Stable branch. Receives changes **only** through release merges from `develop`. |
| `develop` | Integration branch. **All pull requests must target this branch.** |

Short-lived work branches are created from `develop` and merged back into `develop` through pull requests.

### Branch naming

Use the commit type as the branch prefix, followed by a short kebab-case description:

```text
feat/generators-scan
fix/update-endpoint-id-validation
docs/contributing-guide
chore/align-package-versions
style/normalize-endpoint-formatting
```

Rules:

- Use lowercase type prefixes: `feat`, `fix`, `docs`, `chore`, `style`
- Use kebab-case after the slash
- Keep one logical change per branch
- Branch from the latest `develop`
- Delete the branch after the pull request is merged

---

## Development workflow

1. Sync your local `develop` branch with the remote.
2. Create a type-prefixed branch from `develop`.
3. Make focused commits using the commit message format described below.
4. Run the local validation checklist before opening a pull request.
5. Open a pull request **into `develop`**, not `main`.
6. Fill in the pull request description with a summary and a test plan.
7. Address review feedback with additional commits, or squash when agreed during review.

---

## Commit message conventions

We use [Conventional Commits](https://www.conventionalcommits.org/) with an optional scope.

```text
type(scope): short description in English
```

### Allowed types

| Type | Use for |
|------|---------|
| `feat` | New behavior or capability |
| `fix` | Bug fixes |
| `docs` | Documentation only |
| `chore` | Maintenance, tooling, dependencies, repository housekeeping |
| `style` | Formatting or non-functional code style changes |

### Recommended scopes

Use a scope when the change is localized. Omit the scope for cross-cutting changes.

| Scope | When to use |
|-------|-------------|
| `Modules` | `FastSharp.Modules` core library |
| `Models` | `FastSharp.Models` |
| `Generators` | `FastSharp.Generators` |
| `Tests` | `FastSharp.Tests` |
| `Sample` | `Samples/QuickStart` |
| `Docs` | `docs/`, `README.md`, `AGENTS.md`, `llms.txt` |
| `CI` | `.github/workflows` |

### Examples

```text
feat(Generators): change endpoint discovery scan rules
fix(Modules): validate route id against body id on update
docs(Docs): add assembly scanning guide
chore(CI): run tests on pull requests
style(Modules): normalize endpoint handler formatting
feat: add pagination defaults to generated list endpoints
```

### Writing rules

- Write commit messages in English
- Use imperative mood (`add`, `fix`, `update`, not `added`, `fixed`, `updates`)
- Use lowercase type names
- Use PascalCase scope names matching the table above
- Do not add a trailing period to the subject line
- Optionally add a body separated by a blank line for rationale or breaking changes

---

## Pull request guidelines

- Open pull requests against **`develop`**, never against `main` for feature work
- Use a pull request title that follows the same Conventional Commit format as the primary change
- Keep pull requests small and focused
- Link related GitHub issues when applicable (for example, `closes #16`)
- Include or update tests in `FastSharp.Tests` for behavior changes
- Update documentation in `docs/` and/or `README.md` for public API or usage changes
- Keep `Samples/QuickStart/Api.http` aligned when sample routes change

Suggested pull request description:

```markdown
## Summary
- Brief description of the change and why it is needed

## Test plan
- [ ] dotnet build -c Release
- [ ] dotnet test FastSharp.Tests/FastSharp.Tests.csproj -c Release
- [ ] dotnet build Samples/QuickStart/Api.csproj -c Release
- [ ] Manual verification steps, if any
```

---

## Code and documentation standards

Follow the conventions already used in this repository:

- Write code comments and XML documentation in English
- Frame FastSharp as **modules first, endpoints second, CRUD optional**
- Use leading slashes on route prefixes (`"/api"`, `"/products"`)
- Register custom endpoints with `Include<T>()` only
- Prefer minimal, focused diffs over broad refactors
- Keep the project licensed under Apache 2.0 (`LICENSE`)

For architectural context, see:

- [AGENTS.md](AGENTS.md)
- [docs/architecture.md](docs/architecture.md)
- [docs/how-to-fastsharp.md](docs/how-to-fastsharp.md)

---

## Local validation

Run these commands before opening a pull request:

```bash
dotnet restore
dotnet build -c Release
dotnet test FastSharp.Tests/FastSharp.Tests.csproj -c Release
dotnet build Samples/QuickStart/Api.csproj -c Release
```

Pull requests targeting `develop` also run the same checks automatically through [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

Release publishing still runs separately through [`.github/workflows/nuget-publish.yml`](.github/workflows/nuget-publish.yml) when a version tag such as `v1.0.0` is pushed.

---

## Release model

- Day-to-day work merges into `develop`
- `main` is updated only when preparing a release by merging `develop` into `main`
- Releases are tagged with `v*` (for example, `v1.0.0`)
- NuGet publishing is triggered by those tags through the existing workflow

Contributors should not open feature pull requests against `main`.

---

## Getting help

If you are new to the project, start with:

- [README.md](README.md)
- [docs/how-to-fastsharp.md](docs/how-to-fastsharp.md)
- [AGENTS.md](AGENTS.md)

If you are unsure whether a change fits the project direction, open an issue or discuss it in your pull request before investing in a large refactor.
