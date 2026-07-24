# AGENTS.md

FastSharp is a lightweight library for building APIs in C# and ASP.NET Core Minimal APIs.

Its main perspective is:

- **Modules** define the route group and API contract
- **Endpoints** implement behavior as independent classes
- **`AddCRUD`** is an optional shortcut for standard REST operations

This file is written for code agents that need to understand, modify, or extend the repository safely.

## What this repository contains

- `FastSharp.Modules`: core library for modules, endpoint discovery, and generic CRUD mapping
- `FastSharp.Models`: small abstractions shared by consumers, such as `IModel<TKey>` and `PagedResult<T>`
- `FastSharp.Tests`: behavior tests for endpoint discovery and CRUD mapping
- `Samples/QuickStart`: runnable sample app using an in-memory EF Core database

## Primary goals of the project

- Organize APIs by domain modules instead of technical layers
- Make modules the main composition unit and endpoints the main implementation unit
- Keep CRUD generation available but optional
- Reduce boilerplate for Minimal API CRUD endpoints when CRUD is desired
- Keep customization flexible by exposing native Minimal API convention builders (`IEndpointConventionBuilder` at module level and `RouteHandlerBuilder` at CRUD endpoint level)
- Support both generated CRUD endpoints and explicit custom endpoints in the same module

## Critical concepts

### Modules

- `FastSharp.Modules/Core/Module.cs`
- `Module` groups endpoints under a route prefix and supports custom-endpoint-only modules
- `Module<TDbContext>` adds EF Core CRUD support
- `Configure(ModuleConfiguration)` sets the base route group and exposes `IEndpointConventionBuilder` through `Conventions` for shared metadata, policies, filters, and other module-level conventions
- `AddRoutes(RouteGroupBuilder)` composes CRUD, included endpoints, and small inline Minimal API routes; `ConfigureModule(...)` remains compatibility-only
- In the current project language, modules are closer to contracts/composition units than direct request handlers

### CRUD registration

- `AddCRUD<TEntity, TKey>("/products")`
- CRUD is optional; do not assume every module needs it
- Optional overload accepts an explicit id selector for entities that do not implement `IModel<TKey>`
- CRUD configuration is handled through `ICrudEndpoints<TDbContext>`
- Per-endpoint and shared configuration are both supported

### Custom endpoints

- `FastSharp.Modules/Core/IEndpoint.cs`
- `IEndpoint` is the implementation unit for explicit endpoint behavior
- Implement `IEndpoint.Map(RouteGroupBuilder app)`
- Register custom endpoints in a module with `Include<T>()`
- Custom endpoints map on the module route group, not under each CRUD prefix

### Assembly scanning and mapping

- `FastSharp.Modules/DependencyInjection.cs`
- `AddFastSharpEndpoints(...)` registers modules and endpoints from one or more assemblies
- `MapFastSharpEndpoints(...)` resolves registered modules and maps their route groups
- If no assembly is supplied, the calling assembly is used

## Generated CRUD routes

Given:

- `configuration.Prefix = "/api"` in `Configure(...)`
- `AddCRUD<Product, int>("/products")`

FastSharp generates:

- `GET /api/products` (returns `List<Product>`; add `?page=1&pageSize=10` for `PagedResult<Product>`)
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`

This is only one usage mode. Modules can also be custom-endpoint-only.

Example from tests:

- `FastSharp.Tests/Modules/NoContextModule.cs`
- `FastSharp.Tests/Endpoints/NoContextPingEndpoint.cs`

## Important repository conventions

- Route prefixes should use a leading slash
  - Good: `"/api"`, `"/products"`
- Authorization should be applied through native Minimal API chaining (`IEndpointConventionBuilder` for module-level policies, `RouteHandlerBuilder` for generated CRUD endpoint policies)
- Use English for comments and documentation text
- Prefer explicit endpoint registration with `Include<T>()` when possible
- Do not frame the library as CRUD-first; frame it as modules + endpoints first, CRUD optional
- Keep `docs/how-to-fastsharp.md` prominent as the main guide for project structure and adoption paths
- Keep changes minimal and aligned with the existing style

## Where to start, depending on the task

### If you need the public usage model

- `README.md`

### If you need project structure guidance

- `docs/how-to-fastsharp.md`

### If you need the architectural intent

- `docs/architecture.md`
- focus on domain modules, not controllers or technical layers

### If you need configuration and DTO behavior

- `docs/customization.md`
- `docs/validation.md`
- `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
- `FastSharp.Modules/Configuration/CRUDEndpoints.cs`
- `FastSharp.Modules/Core/FastSharpExtensions.cs`
- `FastSharp.Modules/Filters/ValidationFilter.cs`

### If you need runtime mapping behavior

- `FastSharp.Modules/Core/Module.cs`
- `FastSharp.Modules/DependencyInjection.cs`
- `FastSharp.Modules/Core/Endpoints/*.cs`

### If you need a working consumer example

- `Samples/QuickStart/Program.cs`
- `Samples/QuickStart/Modules/Products/ProductsModule.cs`
- `Samples/QuickStart/Modules/Products/Endpoints/UpdateProductsStock.cs`
- `Samples/QuickStart/Api.http`

### If you need a custom-endpoints-only example

- `FastSharp.Tests/Modules/NoContextModule.cs`
- `FastSharp.Tests/Endpoints/NoContextPingEndpoint.cs`

### If you need to scaffold modules, CRUD, endpoints, or validation

- `skills/fastsharp/SKILL.md`
  - repo-local Claude Code skill for convention-aware FastSharp scaffolding (`/fastsharp module|crud|endpoint|validation`); installable by consumers via `npx skills add Henry-cmd325/FastSharp`

### If you need expected behavior before editing core logic

- `FastSharp.Tests/FastSharpEndpointsTests.cs`
- `FastSharp.Tests/Modules/*.cs`

## Repository map for code agents

### Core library

- `FastSharp.Modules/DependencyInjection.cs`
  - assembly scanning and registration
- `FastSharp.Modules/Core/Module.cs`
  - module composition, route grouping, CRUD integration
- `FastSharp.Modules/Core/IEndpoint.cs`
  - custom endpoint contract
- `FastSharp.Modules/Core/IFastModule.cs`
  - internal module mapping contract
- `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
  - CRUD customization API surface
- `FastSharp.Modules/Configuration/CRUDEndpoints.cs`
  - CRUD configuration implementation
- `FastSharp.Modules/Configuration/GenericEndpoints.cs`
  - enum for endpoint selection and disabling
- `FastSharp.Modules/Core/Endpoints/CreateEndpoint.cs`
- `FastSharp.Modules/Core/Endpoints/GetByIdEndpoint.cs`
- `FastSharp.Modules/Core/Endpoints/GetListEndpoint.cs`
- `FastSharp.Modules/Core/Endpoints/UpdateEndpoint.cs`
- `FastSharp.Modules/Core/Endpoints/DeleteEndpoint.cs`
  - concrete generated endpoint implementations

### Shared models

- `FastSharp.Models/IModel.cs`
- `FastSharp.Models/PagedResult.cs`

### Tests

- `FastSharp.Tests/FastSharpEndpointsTests.cs`
  - verifies registration, CRUD mapping, disabled endpoints, custom endpoints

### Sample app

- `Samples/QuickStart/Program.cs`
  - service registration and endpoint mapping
- `Samples/QuickStart/Modules/Products/ProductsModule.cs`
  - example module with standard CRUD, alternate CRUD, and custom endpoint inclusion
- `Samples/QuickStart/Modules/Products/Endpoints/UpdateProductsStock.cs`
  - sample complex included endpoint with FluentValidation integration
- `Samples/QuickStart/Api.http`
  - executable sample requests

### Non-EF/custom-only example

- `FastSharp.Tests/Modules/NoContextModule.cs`
  - example of a module without `DbContext`
- `FastSharp.Tests/Endpoints/NoContextPingEndpoint.cs`
  - example of endpoint-only behavior inside a plain `Module`

## Common agent tasks

### Add a new configurable behavior to generated CRUD endpoints

Likely files:

- `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
- `FastSharp.Modules/Configuration/CRUDEndpoints.cs`
- one or more files under `FastSharp.Modules/Core/Endpoints/`
- tests in `FastSharp.Tests/`

### Change module discovery or mapping

Likely files:

- `FastSharp.Modules/DependencyInjection.cs`
- `FastSharp.Modules/Core/Module.cs`
- tests in `FastSharp.Tests/`

### Improve documentation or examples

Likely files:

- `README.md`
- `docs/how-to-fastsharp.md`
- `docs/*.md`
- `Samples/QuickStart/*`

When updating docs, preserve the current perspective:

- modules first
- endpoints as implementations
- CRUD as optional

### Add a new sample scenario

Likely files:

- `Samples/QuickStart/Program.cs`
- `Samples/QuickStart/Modules/...`
- `Samples/QuickStart/Api.http`

## How to validate changes

- For behavior changes, review and run relevant tests in `FastSharp.Tests`
- For sample changes, verify the sample project still builds and that `Samples/QuickStart/Api.http` remains aligned with the exposed routes
- For documentation-only changes, keep content consistent with the actual public API and sample code

## Known design expectations

- FastSharp is not controller-based; modules compose endpoints rather than handling requests directly
- Modules are the public structural concept; endpoints are the behavioral implementation concept
- CRUD endpoint configuration should remain compatible with native Minimal API metadata chaining
- DTO support is a first-class scenario and should not be treated as an afterthought
- Custom endpoints should remain easy to combine with generated CRUD routes inside the same module
- Custom-endpoint-only modules are a supported scenario and should not be treated as secondary

## Best first reads for an agent

1. `README.md`
2. `FastSharp.Modules/Core/Module.cs`
3. `FastSharp.Modules/DependencyInjection.cs`
4. `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
5. `docs/validation.md`
6. `Samples/QuickStart/Modules/Products/ProductsModule.cs`
7. `FastSharp.Tests/FastSharpEndpointsTests.cs`
