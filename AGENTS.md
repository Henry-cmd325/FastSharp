# AGENTS.md

FastSharp is a small .NET library for building modular ASP.NET Core Minimal APIs. Its main value is generating CRUD endpoints from a domain module with very little boilerplate while still allowing native Minimal API customization.

This file is written for code agents that need to understand, modify, or extend the repository safely.

## What this repository contains

- `FastSharp.Modules`: core library for modules, endpoint discovery, and generic CRUD mapping
- `FastSharp.Models`: small abstractions shared by consumers, such as `IModel<TKey>` and `PagedResult<T>`
- `FastSharp.Tests`: behavior tests for endpoint discovery and CRUD mapping
- `Samples/QuickStart`: runnable sample app using an in-memory EF Core database

## Primary goals of the project

- Reduce boilerplate for Minimal API CRUD endpoints
- Organize APIs by domain modules instead of MVC controllers
- Keep customization flexible by exposing native `RouteHandlerBuilder` configuration points
- Support both generated CRUD endpoints and explicit custom endpoints in the same module

## Critical concepts

### Modules

- `FastSharp.Modules/Module.cs`
- `Module` groups endpoints under a route prefix
- `Module<TDbContext>` adds EF Core CRUD support
- `ConfigureModule("/api", ...)` sets the base route group and shared metadata

### CRUD registration

- `AddCRUD<TEntity, TKey>("/products")`
- Optional overload accepts an explicit id selector for entities that do not implement `IModel<TKey>`
- CRUD configuration is handled through `ICrudEndpoints<TDbContext>`
- Per-endpoint and shared configuration are both supported

### Custom endpoints

- `FastSharp.Modules/IEndpoint.cs`
- Implement `IEndpoint.Map(RouteGroupBuilder app)`
- Register custom endpoints in a module with `Include<T>()`
- `IncludeNamespace<T>()` exists, but explicit `Include<T>()` is the preferred pattern

### Assembly scanning and mapping

- `FastSharp.Modules/DependencyInjection.cs`
- `AddFastSharpEndpoints(...)` registers modules and endpoints from one or more assemblies
- `MapFastSharpEndpoints(...)` resolves registered modules and maps their route groups
- If no assembly is supplied, the calling assembly is used

## Generated CRUD routes

Given:

- `ConfigureModule("/api", ...)`
- `AddCRUD<Product, int>("/products")`

FastSharp generates:

- `GET /api/products/paged`
- `GET /api/products`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`

## Important repository conventions

- Route prefixes should use a leading slash
  - Good: `"/api"`, `"/products"`
- Authorization should be applied through native Minimal API chaining on `RouteHandlerBuilder`
- Use English for comments and documentation text
- Prefer explicit endpoint registration with `Include<T>()` when possible
- Keep changes minimal and aligned with the existing style

## Where to start, depending on the task

### If you need the public usage model

- `README.md`

### If you need the architectural intent

- `docs/architecture.md`

### If you need configuration and DTO behavior

- `docs/customization.md`
- `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
- `FastSharp.Modules/Configuration/CRUDEndpoints.cs`

### If you need runtime mapping behavior

- `FastSharp.Modules/Module.cs`
- `FastSharp.Modules/DependencyInjection.cs`
- `FastSharp.Modules/Endpoints/*.cs`

### If you need a working consumer example

- `Samples/QuickStart/Program.cs`
- `Samples/QuickStart/Modules/Products/ProductsModule.cs`
- `Samples/QuickStart/Api.http`

### If you need expected behavior before editing core logic

- `FastSharp.Tests/FastSharpEndpointsTests.cs`
- `FastSharp.Tests/Modules/*.cs`

## Repository map for code agents

### Core library

- `FastSharp.Modules/DependencyInjection.cs`
  - assembly scanning and registration
- `FastSharp.Modules/Module.cs`
  - module composition, route grouping, CRUD integration
- `FastSharp.Modules/IEndpoint.cs`
  - custom endpoint contract
- `FastSharp.Modules/IFastModule.cs`
  - internal module mapping contract
- `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
  - CRUD customization API surface
- `FastSharp.Modules/Configuration/CRUDEndpoints.cs`
  - CRUD configuration implementation
- `FastSharp.Modules/Configuration/GenericEndpoints.cs`
  - enum for endpoint selection and disabling
- `FastSharp.Modules/Endpoints/CreateEndpoint.cs`
- `FastSharp.Modules/Endpoints/GetByIdEndpoint.cs`
- `FastSharp.Modules/Endpoints/GetListEndpoint.cs`
- `FastSharp.Modules/Endpoints/GetPagedEndpoint.cs`
- `FastSharp.Modules/Endpoints/UpdateEndpoint.cs`
- `FastSharp.Modules/Endpoints/DeleteEndpoint.cs`
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
- `Samples/QuickStart/Modules/Products/Endpoints/CheckProductStock.cs`
  - sample custom endpoint
- `Samples/QuickStart/Api.http`
  - executable sample requests

## Common agent tasks

### Add a new configurable behavior to generated CRUD endpoints

Likely files:

- `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
- `FastSharp.Modules/Configuration/CRUDEndpoints.cs`
- one or more files under `FastSharp.Modules/Endpoints/`
- tests in `FastSharp.Tests/`

### Change module discovery or mapping

Likely files:

- `FastSharp.Modules/DependencyInjection.cs`
- `FastSharp.Modules/Module.cs`
- tests in `FastSharp.Tests/`

### Improve documentation or examples

Likely files:

- `README.md`
- `docs/*.md`
- `Samples/QuickStart/*`

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
- CRUD endpoint configuration should remain compatible with native Minimal API metadata chaining
- DTO support is a first-class scenario and should not be treated as an afterthought
- Custom endpoints should remain easy to combine with generated CRUD routes inside the same module

## Best first reads for an agent

1. `README.md`
2. `FastSharp.Modules/Module.cs`
3. `FastSharp.Modules/DependencyInjection.cs`
4. `FastSharp.Modules/Configuration/ICrudEndpoints.cs`
5. `Samples/QuickStart/Modules/Products/ProductsModule.cs`
6. `FastSharp.Tests/FastSharpEndpointsTests.cs`
