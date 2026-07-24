# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore and build
dotnet restore
dotnet build -c Release

# Run tests
dotnet test FastSharp.Tests/FastSharp.Tests.csproj -c Release

# Run a single test
dotnet test FastSharp.Tests/FastSharp.Tests.csproj -c Release --filter "FullyQualifiedName~<TestMethodName>"

# Build the sample project
dotnet build Samples/QuickStart/Api.csproj -c Release

# Run the sample app
dotnet run --project Samples/QuickStart/Api.csproj
```

## Architecture

FastSharp is a NuGet library for building ASP.NET Core Minimal APIs organized by **domain modules**. The framing is: **modules first, endpoints second, CRUD optional**.

### Project layout

- **`FastSharp.Modules`** — core library: module base classes, CRUD endpoint generation, assembly scanning, DI extensions, FluentValidation filter
- **`FastSharp.Models`** — shared abstractions (`IModel<TKey>`, `PagedResult<T>`); add this to projects that don't need the full core
- **`FastSharp.Generators`** — Roslyn source generator that emits a per-assembly `IFastSharpAssemblyRegistry`, used at startup to discover modules and endpoints without reflection scanning
- **`FastSharp.Tests`** — integration tests using `WebApplicationFactory`-style in-memory test host; all behavior tests live here
- **`Samples/QuickStart`** — runnable sample app; `Api.http` contains executable sample requests

### Core model

A **module** (`Module` or `Module<TDbContext>`) defines a route group and composes endpoints:
- `Configure(ModuleConfiguration)` sets the route prefix and group-level OpenAPI metadata
- `AddRoutes(RouteGroupBuilder)` contains `AddCRUD<TEntity, TKey>("/items")`, `Include<TEndpoint>()`, and small inline routes
- `ConfigureModule(...)` remains available only for constructor-style compatibility
- Custom `IEndpoint` types implement `Map(RouteGroupBuilder app)` and are the behavioral unit

**Assembly scanning** (`DependencyInjection.cs`):
- `AddFastSharpEndpoints(assembly)` registers modules, endpoints, and validators from one or more assemblies; defaults to calling assembly
- `MapFastSharpEndpoints()` resolves registered modules and maps their route groups once per application/assembly
- Both methods require the assembly to have a source-generated `IFastSharpAssemblyRegistry` (produced by `FastSharp.Generators`); passing an assembly without one throws `InvalidOperationException`

**CRUD generation** (`FastSharp.Modules/Core/Endpoints/`, `Configuration/`):
- Entities used with the parameterless `AddCRUD` overload must implement `IModel<TKey>`
- The id-selector overload (`AddCRUD<TEntity, TKey>(prefix, e => e.Id, ...)`) works with plain POCOs
- `crud.DisableEndpoint(GenericEndpoint.GetList)` disables individual generated routes
- Per-endpoint DTOs: `crud.GetList<ProductDto>(...)`, `crud.Create<RequestDto, ResponseDto>(...)`; or `crud.ConfigureAll<Dto>()` for all at once
- Each per-endpoint method returns `RouteHandlerBuilder` for native Minimal API chaining (`.WithDescription(...)`, `.RequireAuthorization()`, etc.)

**Validation** (`FastSharp.Modules/Filters/ValidationFilter.cs`):
- Apply `.WithValidation<T>()` on any route handler to activate FluentValidation
- Validators are auto-registered via `AddValidatorsFromAssemblies` when `AddFastSharpEndpoints` runs
- If no `IValidator<T>` is registered, the filter is a no-op

### Key conventions

- Route prefixes must use a leading slash: `"/api"`, `"/products"`
- Authorization is wired through native Minimal API chaining on `RouteHandlerBuilder`, not through any FastSharp-specific abstraction
- Code comments and XML docs must be in English
- Documentation edits go to `docs/*.md` / `README.md` before XML doc changes, unless XML is specifically requested

## Branching and commits

- All PRs target `develop`, never `main`; `main` receives only release merges
- Branch naming: `feat/`, `fix/`, `docs/`, `chore/`, `style/` prefix with kebab-case description
- Commit format: `type(Scope): short description` (Conventional Commits, imperative mood, English)
- Allowed scopes: `Modules`, `Models`, `Generators`, `Tests`, `Sample`, `Docs`, `CI`
