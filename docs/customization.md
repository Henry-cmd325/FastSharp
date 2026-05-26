# Customization

[Back to README](../README.md)

## Index

- [Modular architecture](architecture.md)

FastSharp can be customized at two levels:

- **Module level** with `ConfigureModule(...)`
- **CRUD level** with `AddCRUD(...)` and `ICrudEndpoints<TDbContext>`

Remember the current perspective of the library:

- **Modules** define the route group and the contract
- **Endpoints** implement behavior
- **`AddCRUD`** is optional

Customization of generated CRUD endpoints is done in your module's constructor via the `AddCRUD` method. The first parameter is the base route (use a **leading slash**, e.g. `"/products"`). The second (optional) gives you `ICrudEndpoints<TDbContext>`, letting you apply shared configuration, disable endpoints, and define DTO contracts.

To configure the module base group (applying options to all endpoints in the group), use `ConfigureModule`. This lets you add metadata or policies at the group level.

```csharp
using FastSharp.Modules.Core;
using FastSharp.Modules.Configuration;
using YourProject.Models;
using YourProject.Data;

public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", module => module
            .WithDescription("Endpoints for managing products in the inventory"));

        AddCRUD<Product, int>("/products", crud =>
        {
            // Example 1: Disable an endpoint
            crud.DisableEndpoint(GenericEndpoint.GetList);

            // Example 2: Apply metadata to all CRUD endpoints
            crud.ConfigureAll(endpoints => endpoints
                .WithDescription("Products CRUD endpoint"));
        });
    }
}
```

The `endpoint` parameter in `ConfigureAll`, `Get`, `GetList`, `Create`, `Update`, and `Delete` is a `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (or similar, depending on the .NET version). This gives access to Minimal APIs extension methods like `WithOpenApi`, `RequireAuthorization`, `Accepts`, and `Produces`.

---

# Usage modes

FastSharp can be used in different ways depending on what the developer needs.

## 1) CRUD-only

```csharp
public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", module => module.WithTags("Products"));
        AddCRUD<Product, int>("/products");
    }
}
```

## 2) CRUD + custom endpoints

```csharp
public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", module => module.WithTags("Products"));
        AddCRUD<Product, int>("/products");
        Include<CheckStock>();
    }
}
```

## 3) Custom endpoints only

```csharp
public class UtilityModule : Module
{
    public UtilityModule()
    {
        ConfigureModule("/api", module => module.WithTags("Utility"));
        Include<StatusEndpoint>();
    }
}
```

This mode does not require EF Core or `AddCRUD(...)`.

---

# Validation

Custom endpoints can use FluentValidation through `WithValidation<T>()`.

```csharp
using FastSharp.Modules.Core;
using FluentValidation;

builder.Services.AddScoped<IValidator<CreateProductRequest>, CreateProductRequestValidator>();

public record CreateProductRequest(string Name);

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public sealed class CreateProductEndpoint : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapPost("/products", (CreateProductRequest request) => Results.Ok(request))
            .WithValidation<CreateProductRequest>();
    }
}
```

See [Validation with FluentValidation](validation.md) for the full flow and response behavior.

---

# DTO contracts

## 1) Single DTO for all CRUD endpoints

```csharp
AddCRUD<Product, int>("/products", crud => crud
    .ConfigureAll<ProductDto>());
```

Resulting contracts:
- `GET /{id}` -> `ProductDto`
- `GET /` -> `List<ProductDto>` (add `?page=1&pageSize=10` for `PagedResult<ProductDto>`)
- `POST /` -> request `ProductDto`, response `ProductDto`
- `PUT /{id}` -> request `ProductDto`

## 2) Separate write/read DTOs

```csharp
AddCRUD<Product, int>("/products", options => options
    .ConfigureAll<ProductWriteDto, ProductReadDto>());
```

Resulting contracts:
- `GET /{id}` -> `ProductReadDto`
- `GET /` -> `List<ProductReadDto>` (add `?page=1&pageSize=10` for `PagedResult<ProductReadDto>`)
- `POST /` -> request `ProductWriteDto`, response `ProductReadDto`
- `PUT /{id}` -> request `ProductWriteDto`

---

# Custom endpoints

[Back to README](../README.md)

## Index

- [Modular architecture](architecture.md)

Besides CRUD endpoints, you can create your own by implementing `IEndpoint`. These must be registered within a module to be mapped.

Important: custom endpoints are mapped on the **module route group**, not inside each `AddCRUD(...)` route prefix.

## 1) Define the endpoint

```csharp
// YourProject/Slices/Products/Endpoints/CheckStock.cs
using FastSharp.Modules.Core;
using Microsoft.AspNetCore.Mvc;

public class CheckStock : IEndpoint
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id}/stock", async ([FromRoute] int id) =>
        {
            return Results.Ok($"Product {id} has 10 units in stock.");
        })
        .WithTags("Stock");
    }
}
```

## 2) Include in the module

Use `Include<T>()` for a single endpoint or `IncludeNamespace<T>()` for all endpoints in a namespace. Prefer `Include<T>()` when possible because it is more explicit.

```csharp
using YourProject.Data;
using YourProject.Models;

public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", module => module.WithTags("Products"));
        Include<CheckStock>();
    }
}
```

If the endpoint maps:

- `group.MapGet("/{id}/stock", ...)`

and the module prefix is:

- `ConfigureModule("/api", ...)`

the final route is:

- `GET /api/{id}/stock`

not:

- `GET /api/products/{id}/stock`

unless the endpoint itself includes `/products` in its route template.

The default module prefix is `/api` when you do not call `ConfigureModule(...)`.

## 3) Apply module-wide metadata to custom endpoints

Because custom endpoints are mapped on the module route group, they share the group-level metadata configured in `ConfigureModule(...)`.

```csharp
public class ProductsModule : Module
{
    public ProductsModule()
    {
        ConfigureModule("/api", module => module
            .WithTags("Products")
            .WithDescription("Product module endpoints"));

        Include<CheckStock>();
    }
}
```
