# FastSharp API Vision

FastSharp should evolve toward a route-first API that keeps ASP.NET Core Minimal APIs recognizable while reducing the ceremony required to generate, group, and customize endpoints.

This document describes the intended direction. The API shown below is **aspirational and not fully implemented yet**.

## Target experience

```csharp
public sealed class ProductsModule : Module<AppDbContext>
{
    protected override void Configure(ModuleConfiguration config)
    {
        config.WithPrefix("/api/products")
              .WithTags("Products");
    }

    protected override void AddRoutes(RouteGroupBuilder routes)
    {
        // 1. General CRUD by convention
        routes.MapCRUD<Product, int>();

        // 2. A submodule focused on one use case or endpoint
        routes.AddSubmodule<UpdateProductEndpoint>();

        // 3. A submodule that groups related endpoints
        routes.AddSubmodule<ProductCategoriesGroup>("/categories");

        // 4. Direct mapping for genuinely trivial behavior
        routes.MapGet("/{id:int}/quick-view", (int id) => ...);
    }
}
```

The module communicates two responsibilities:

- `Configure` defines shared module conventions.
- `AddRoutes` composes every route exposed by the module.

## Design goals

### Keep Minimal APIs recognizable

`RouteGroupBuilder` remains the route composition surface. Developers can use native methods such as `MapGet`, `MapPost`, authorization, filters, metadata, and OpenAPI conventions without learning a parallel routing framework.

### Make endpoint generation the primary value

`MapCRUD<TEntity, TKey>()` should provide the shortest path from an entity to a conventional CRUD API. Configuration should remain available when DTOs, validation, pagination, authorization, or individual endpoint behavior must differ from the defaults.

### Scale from trivial routes to focused components

FastSharp should support three levels of route composition without forcing every endpoint into the same structure:


| Mechanism                                     | Intended use                                                          |
| --------------------------------------------- | --------------------------------------------------------------------- |
| `routes.MapGet(...)` and other native methods | Behavior that remains clear in a few lines                            |
| `routes.AddSubmodule<T>()`                    | A focused use case or endpoint with its own dependencies and behavior |
| `routes.AddSubmodule<T>(prefix)`              | A cohesive group of related endpoints under a nested route prefix     |


The choice should follow behavioral complexity, not an arbitrary requirement that every route needs its own class.

### Preserve module boundaries

CRUD routes, submodules, and directly mapped routes must remain inside the parent module's route group. They should inherit its prefix, tags, authorization, filters, and other conventions unless explicitly overridden.

### Preserve native dependency injection

Submodules should be resolved through ASP.NET Core dependency injection. Route handlers should continue to support native parameter binding and service injection.

## Intended concepts

### Module configuration

`ModuleConfiguration` should offer a fluent API for shared conventions:

```csharp
protected override void Configure(ModuleConfiguration config)
{
    config.WithPrefix("/api/products")
          .WithTags("Products");
}
```

Future configuration methods should wrap native endpoint conventions rather than replace them.

### Conventional CRUD

CRUD generation should read naturally from the route group:

```csharp
routes.MapCRUD<Product, int>();
```

This makes generated endpoints part of route composition instead of making CRUD appear to configure the module itself.

The existing FastSharp capabilities remain important:

- Request and response DTOs
- Validation
- Pagination
- Endpoint disabling
- Per-endpoint metadata and authorization
- Explicit key selectors for entities that do not implement `IModel<TKey>`

### Submodules

A submodule is an endpoint composition unit nested inside a module. It may represent either one substantial use case or a cohesive group of related routes, in order to recognize them easier we are going to use a new convention, Submodules with one endpoint will end with Endpoint and Submodules with two ore more endpoints will end with Group.

```csharp
routes.AddSubmodule<UpdateProductStockEndpoint>();
routes.AddSubmodule<ProductCategoriesGroup>("/categories");
```

Submodules should not introduce a second application architecture. Their purpose is to keep large modules readable while preserving the parent module as the public domain boundary.

## Compatibility direction

The evolution toward this API should be additive and suitable for incremental adoption.

Existing applications using these APIs should continue to work during the transition:

```csharp
ConfigureModule(...);
AddCRUD<TEntity, TKey>(...);
Include<TEndpoint>();
```

New APIs should initially delegate to the same internal mapping engine. Existing applications must not be forced to rewrite every module when updating FastSharp.

## Proposed evolution

1. Add fluent `ModuleConfiguration` methods such as `WithPrefix` and `WithTags`.
2. Add `MapCRUD<TEntity, TKey>()` extensions for `RouteGroupBuilder` while retaining `AddCRUD` compatibility.
3. Define the submodule contract and lifecycle.
4. Add `AddSubmodule<T>()` with optional nested prefixes and dependency injection support.
5. Verify inherited conventions, route ordering, duplicate detection, and mapping failure behavior.
6. Migrate samples and templates before recommending the new API as the default.
7. Deprecate older syntax only after a documented migration period, if deprecation remains beneficial.

## Non-goals

- Replacing ASP.NET Core routing, dependency injection, filters, or endpoint conventions
- Requiring a submodule class for every trivial route
- Making CRUD mandatory for every module
- Turning modules into request handlers
- Breaking existing constructor-style modules during adoption
- Copying another library's API when it does not improve endpoint generation or developer speed

## Success criteria

The direction is successful when:

- A developer familiar with Minimal APIs can understand a FastSharp module without studying a separate routing model.
- Conventional CRUD requires minimal code but remains deeply customizable.
- Small routes remain small.
- Complex use cases have an obvious extraction path.
- Nested endpoint groups inherit parent module conventions predictably.
- Existing FastSharp consumers can upgrade without rewriting their modules.
