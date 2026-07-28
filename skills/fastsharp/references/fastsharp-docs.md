# FastSharp Skill Reference

Use this file as the standalone FastSharp reference when scaffolding consumer projects. It must not depend on files from the FastSharp repository being present in the target app.

## Public usage model

- FastSharp is a lightweight C# ASP.NET Core Minimal APIs library.
- The mental model is **modules first, endpoints as implementations, CRUD optional**.
- Install `FastSharp.Modules` for the module and endpoint framework; it brings `FastSharp.Models` transitively. Install `FastSharp.Models` directly only when a project needs standalone abstractions without the framework.
- `FastSharp.Modules` contains the core module, endpoint, validation, and CRUD support.
- `FastSharp.Models` contains shared abstractions such as `IModel<TKey>` and `PagedResult<T>`.
- CRUD generation is a shortcut, not the default framing for every module.

Source: `README.md`

## Project structure

- Start simple; FastSharp does not require one fixed folder layout.
- Small apps can keep `Modules/`, `Data/`, DTOs, endpoints, and `Program.cs` in one project.
- Growing apps can group modules under an infrastructure or feature area while keeping one deployable API.
- Advanced apps can move domains into separate assemblies and pass those assemblies to FastSharp registration/mapping.
- Keep module-local code close together: module class, `Endpoints/`, `Dtos/`, and related request/response contracts.
- Move to separate assemblies only when domains need stronger separation or reuse.

```text
MyApi/
├── Modules/
│   └── Products/
│       ├── ProductsModule.cs
│       ├── Endpoints/
│       └── Dtos/
├── Data/
└── Program.cs
```

Source: `docs/how-to-fastsharp.md`

## Architecture intent

- A module represents a domain or feature boundary.
- `Module` is for custom-endpoints-only modules and does not require EF Core.
- `Module<TDbContext>` adds EF Core-backed CRUD support while still allowing custom endpoints.
- A module composes routes and metadata; it should not become a controller, service, or request handler.
- `IEndpoint` classes implement explicit behavior through `Map(RouteGroupBuilder app)`.
- Custom endpoints are mapped on the module route group, not inside each CRUD prefix.
- `Include<TEndpoint>()` registers a custom endpoint implementation inside a module.
- A single module can contain multiple CRUD registrations and multiple custom endpoints.

Source: `docs/architecture.md`, `FastSharp.Modules/Core/Module.cs`

## CRUD configuration & DTOs

- `AddCRUD<TEntity, TKey>(routePrefix, configure?)` requires `TEntity : IModel<TKey>`.
- Plain POCO entities use `AddCRUD<TEntity, TKey>(routePrefix, entity => entity.Id, configure?)`.
- CRUD route prefixes should use a leading slash, e.g. `"/products"`.
- Generated CRUD routes combine the module prefix plus the CRUD prefix.
- Generated operations are list, get by id, create, update, and delete.
- Use `DisableEndpoint(GenericEndpoint.X)` to skip one generated operation.
- Use `ConfigureAll(...)` for shared `RouteHandlerBuilder` metadata such as tags, descriptions, authorization, `Produces`, or OpenAPI configuration.
- Use DTO overloads to change contracts without changing the entity type.
- FastSharp uses the Mapster library to map between entities and DTOs in generated CRUD endpoints.
- `GetList` returns a bounded list by default and supports `?page=` / `?pageSize=` pagination; page size can be capped globally or per endpoint.

```csharp
AddCRUD<Product, int>("/products", crud => crud.ConfigureAll<ProductDto>());

AddCRUD<Product, int>("/products/alternative", p => p.Id, crud =>
{
    crud.DisableEndpoint(GenericEndpoint.GetList);
    crud.GetList<ProductDto>(endpoint => endpoint.WithTags("GetList"));
    crud.Create<ProductRequest, ProductDto>(endpoint => endpoint.WithTags("Create"));
});
```

Source: `docs/customization.md`, `Samples/QuickStart/Modules/Products/ProductsModule.cs`, `FastSharp.Modules/Configuration/ICrudEndpoints.cs`, `FastSharp.Modules/Core/Module.cs`

## Validation flow

- FastSharp validation is for custom Minimal API endpoints built with `IEndpoint`.
- The route handler must opt in with `.WithValidation<TRequest>()` from `FastSharp.Modules.Core`.
- `TRequest` must match the bound request argument type.
- Validators are `FluentValidation.IValidator<TRequest>` implementations.
- `AddFastSharpEndpoints(...)` registers validators from the scanned assemblies automatically through FluentValidation assembly scanning.
- Validators can also be registered manually in DI.
- If a validator exists and validation fails, the endpoint returns `400 Bad Request` with a validation problem payload.
- If no validator is registered for `TRequest`, validation does nothing and the request continues.

```csharp
app.MapPost("/update-stock/{id}", ([FromBody] UpdateProductStock request) => Results.NoContent())
    .WithValidation<UpdateProductStock>();
```

Source: `docs/validation.md`, `Samples/QuickStart/Modules/Products/Endpoints/UpdateProductsStock.cs`

## Source-generated registry (self-contained)

FastSharp resolves module and endpoint metadata through a source-generated registry, not runtime reflection scanning. The `FastSharp.Generators` source generator emits a per-assembly `IFastSharpAssemblyRegistry` that registers itself through a `ModuleInitializer`; `AddFastSharpEndpoints(...)` and `MapFastSharpEndpoints(...)` resolve metadata from that store. `FastSharp.Modules` delivers the generator as an analyzer, so normal package consumers should not add a separate `FastSharp.Generators` reference.

Consequence: every assembly containing FastSharp modules or endpoints must reference `FastSharp.Modules` so the analyzer generates its registry. Otherwise `FastSharpAssemblyRegistryStore.GetRequiredRegistry` throws `InvalidOperationException` at startup. If a module lives in a different assembly than `Program.cs`, reference `FastSharp.Modules` from that assembly before passing it to the registration and mapping methods.

Source: `docs/assembly-scanning.md`, `docs/ADR/001-registry-lifecycle.md`

## Working sample

- Register the app `DbContext` before mapping FastSharp CRUD endpoints.
- Call `builder.Services.AddFastSharpEndpoints()` during service registration.
- Call `app.MapFastSharpEndpoints()` after building the app.
- Add OpenAPI/Swagger separately if the consumer wants docs/UI.
- With no assemblies passed, FastSharp targets the calling assembly; pass explicit assemblies when modules live elsewhere.

```csharp
builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseInMemoryDatabase("fastsharp-demo"));

builder.Services.AddFastSharpEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapFastSharpEndpoints();
app.MapOpenApi();
```

Source: `Samples/QuickStart/Program.cs`

## Module + CRUD sample

- Inherit from `Module<TDbContext>` when the module uses generated CRUD.
- Configure module-level metadata and policies once with `ConfigureModule("/api", ...)`.
- Add standard CRUD with `AddCRUD<TEntity, TKey>(...)`.
- Apply DTOs with `ConfigureAll<TDto>()` for simple read/write DTO parity.
- Use the id-selector overload for entities that do not implement `IModel<TKey>`.
- Include custom endpoints explicitly with `Include<TEndpoint>()`.
- Custom endpoints share the module prefix and group metadata.
- `ConfigureModule` exposes `IEndpointConventionBuilder` for shared Minimal API conventions; `IEndpoint.Map(RouteGroupBuilder app)` receives the route group for concrete route mapping.

```csharp
public class ProductsModule : Module<ApiDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", opt => opt
            .WithTags("Products")
            .WithDescription("Endpoints of products module"));

        AddCRUD<Product, int>("/products", crud => crud.ConfigureAll<ProductDto>());

        AddCRUD<Product, int>("/products/alternative", p => p.Id, crud =>
        {
            crud.DisableEndpoint(GenericEndpoint.GetList);
            crud.GetList<ProductDto>(endpoint => endpoint.WithTags("GetList"));
            crud.Create<ProductRequest, ProductDto>(endpoint => endpoint.WithTags("Create"));
        });

        Include<CheckProductStock>();
        Include<UpdateProductsStock>();
    }
}
```

Source: `Samples/QuickStart/Modules/Products/ProductsModule.cs`

## Validation endpoint sample

- Keep the request contract and validator near the custom endpoint when that improves module cohesion.
- Use Minimal API binding attributes such as `[FromServices]`, `[FromRoute]`, and `[FromBody]` as usual.
- Validate route/body consistency in the handler when both carry the same identifier.
- Chain `.WithValidation<TRequest>()` on the mapped route before or alongside normal Minimal API metadata such as `.WithTags(...)`.

```csharp
public class UpdateProductsStock : IEndpoint
{
    public record UpdateProductStock(int Id, string Name, int Quantity);

    public class UpdateProductStockValidator : AbstractValidator<UpdateProductStock>
    {
        public UpdateProductStockValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Quantity).NotEqual(0);
        }
    }

    public void Map(RouteGroupBuilder app)
    {
        app.MapPost("/update-stock/{id}", async ([FromRoute] int id, [FromBody] UpdateProductStock product) =>
        {
            if (id != product.Id) return Results.BadRequest("ID in route does not match ID in body");
            return Results.NoContent();
        })
        .WithValidation<UpdateProductStock>()
        .WithTags("Custom");
    }
}
```

Source: `Samples/QuickStart/Modules/Products/Endpoints/UpdateProductsStock.cs`

## Minimal Checks

- Module prefixes, CRUD prefixes, and custom endpoint templates use leading slashes.
- Modules define the route group and API contract; endpoints implement behavior.
- CRUD is added only when standard REST operations are useful.
- `AddCRUD<TEntity, TKey>` overloads without an id selector are used only for entities implementing `IModel<TKey>`.
- Entities without `IModel<TKey>` use `AddCRUD<TEntity, TKey>(route, entity => entity.Id, ...)`.
- Custom endpoints are included with `Include<TEndpoint>()` and map on the module route group.
- Validation on custom endpoints uses `.WithValidation<TRequest>()`; registering a validator alone is not enough.
- Every assembly containing FastSharp modules or endpoints references `FastSharp.Modules` so its analyzer generates the required registry; normal consumers do not reference `FastSharp.Generators` separately.
- Generated comments and XML docs are in English.

## Minimal Examples

```csharp
// Module with explicit route group and a custom endpoint
ConfigureModule("/api", options => options.WithTags("Products"));
Include<CheckProductStock>();

// AddCRUD with an id selector (entity does not implement IModel<TKey>)
AddCRUD<Product, int>("/products", product => product.Id);

// AddCRUD without a selector requires the entity to implement IModel<TKey>
// AddCRUD<ProductModel, Guid>("/products");
```

```csharp
public sealed class CheckProductStock : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/{id}/stock", ([FromRoute] int id) => Results.Ok());
    }
}
```
