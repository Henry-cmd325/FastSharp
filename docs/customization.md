# Customization

[Back to README](../README.md)

## Index

- [Modular architecture](architecture.md)

Customization of CRUD endpoints is done in your module's constructor via the `AddCRUD` method. The first parameter is the base route and the second (optional) gives you `ICrudEndpoints<TDbContext>`, letting you apply shared configuration, disable endpoints, and define DTO contracts.

To configure the module base group (applying options to all endpoints in the group), use `ConfigureModule`. This lets you add metadata or policies at the group level.

```csharp
using FastSharp.Modules;
using FastSharp.Modules.Configuration;
using YourProject.Models;
using YourProject.Data;

public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        ConfigureModule(module =>
        {
            module.WithDescription("Endpoints for managing products in the inventory");
        });

        AddCRUD<Product, int>("/products", crud =>
        {
            // Example 1: Disable an endpoint
            crud.DisableEndpoint(GenericEndpoint.GetList);

            // Example 2: Apply metadata to all CRUD endpoints
            crud.ConfigureAll(endpoints =>
            {
                endpoints.WithDescription("Products CRUD endpoint");
            });
        });
    }
}
```

The `endpoint` parameter in `ConfigureAll`, `Get`, `GetList`, `GetPaged`, `Create`, `Update`, and `Delete` is a `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (or similar, depending on the .NET version). This gives access to Minimal APIs extension methods like `WithOpenApi`, `RequireAuthorization`, `Accepts`, and `Produces`.

---

# DTO contracts

## 1) Single DTO for all CRUD endpoints

```csharp
AddCRUD<Product, int>("/products", crud =>
{
    crud.ConfigureAll<ProductDto>();
});
```

Resulting contracts:
- `GET /{id}` -> `ProductDto`
- `GET /` -> `List<ProductDto>`
- `GET /paged` -> `PagedResult<ProductDto>`
- `POST /` -> request `ProductDto`, response `ProductDto`
- `PUT /{id}` -> request `ProductDto`

## 2) Separate write/read DTOs

```csharp
AddCRUD<Product, int>("/products", options =>
{
    options.ConfigureAll<ProductWriteDto, ProductReadDto>();
});
```

Resulting contracts:
- `GET /{id}` -> `ProductReadDto`
- `GET /` -> `List<ProductReadDto>`
- `GET /paged` -> `PagedResult<ProductReadDto>`
- `POST /` -> request `ProductWriteDto`, response `ProductReadDto`
- `PUT /{id}` -> request `ProductWriteDto`

---

# Custom endpoints

[Back to README](../README.md)

## Index

- [Modular architecture](architecture.md)
- [Basic usage](basic-usage.md)

Besides CRUD endpoints, you can create your own by implementing `IEndpoint`. These must be registered within a module to be mapped. Custom endpoints are nested under the module's route.

## 1) Define the endpoint

```csharp
// YourProject/Slices/Products/Endpoints/CheckStock.cs
using FastSharp.Modules;
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

Use `Include<T>()` for a single endpoint or `IncludeNamespace<T>()` for all endpoints in a namespace, we recommend using Include<T>() always since it's more explicit,
but if you want to move faster you can use it.

```csharp
using YourProject.Data;
using YourProject.Models;

public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        IncludeNamespace<CheckStock>();
        // Include<CheckStock>();
    }
}
```

This will result in a new endpoint: `GET /api/products/{id}/stock`.