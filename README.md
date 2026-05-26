# FastSharp

[![NuGet](https://img.shields.io/nuget/v/FastSharp.Modules?logo=nuget)](https://www.nuget.org/packages/FastSharp.Modules)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
[![Publish NuGet packages](https://github.com/Henry-cmd325/FastSharp/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/Henry-cmd325/FastSharp/actions/workflows/nuget-publish.yml)
[![Stars](https://img.shields.io/github/stars/Henry-cmd325/FastSharp?logo=github&style=flat)](https://github.com/Henry-cmd325/FastSharp)

**FastSharp** is a lightweight library for building APIs in C# and ASP.NET Core (Minimal APIs).

It organizes your application using **Modules (contracts)** and **Endpoints (implementations)**, so you can structure your API by domain instead of technical layers.  
You can also generate full CRUD endpoints in one line — but that's optional.

---

## Why FastSharp?

Minimal APIs are flexible, but as your project grows they often become:

- Repetitive  
- Unstructured  
- Hard to scale  

FastSharp solves this with a simple model:

- **Modules** → define the route group and API contract  
- **Endpoints (`IEndpoint`)** → implement behavior as independent classes  
- **`AddCRUD`** → optional shortcut for standard REST operations  

```csharp
// Inside a module constructor, one call maps 6 REST endpoints backed by EF Core
AddCRUD<Product, int>("/products");
```

No controllers. No repetition. Just modules organized by domain.

---

## Installation

```bash
dotnet add package FastSharp.Modules
dotnet add package FastSharp.Models
```

> `FastSharp.Modules` is the core. `FastSharp.Models` contains only the model interfaces — add it to projects that don't need the full core.

⚠️ FastSharp is currently in **beta**. APIs may change between versions.
---

## Quick Start

The minimum setup is **four code files** (steps 2–5 below) plus package restore. This example uses an in-memory database so you can run it immediately.

> 🧠 **Need help choosing a project structure?** See [How to FastSharp](docs/how-to-fastsharp.md) for the recommended ways to organize a FastSharp application as it grows.

**1. Install the dependencies**
```bash
dotnet add package FastSharp.Modules
dotnet add package FastSharp.Models
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

**2. Your model**

```csharp
// Models/Product.cs
using FastSharp.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**3. Your DbContext**

```csharp
// Data/ApiDbContext.cs
using Microsoft.EntityFrameworkCore;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
}
```

**4. Your module**

```csharp
// Modules/Products/ProductsModule.cs
using yourproject.Context;
using yourproject.Context.Models;
using yourproject.Modules.Products.Dtos;
using yourproject.Modules.Products.Endpoints;
using FastSharp.Modules.Core;
using FastSharp.Modules.Configuration;

namespace yourproject.Modules.Products;

public class ProductsModule : Module<ApiDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("/api", opt => opt
            .WithTags("Productos")
            .WithDescription("Endpoints of products module")
        );

        // Use a manual Id selector for entities that do not implement IModel<int>.
        AddCRUD<Product, int>("/products/alternative", p => p.Id, crud =>
        {
            crud.DisableEndpoint(GenericEndpoint.GetList);
            
            crud.GetList<ProductDto>((endpoint) => endpoint
                .WithDescription("Retrieves a list of products (use ?page and ?pageSize for pagination)")
                .WithTags("GetList")
            );

            crud.Create<ProductRequest, ProductDto>((endpoint) => endpoint
                .WithDescription("Creates a new product")
                .WithTags("Create")
            );
        });

        // Declare custom endpoints for this module (implemented via IEndpoint)
        //Include<CheckProductStock>();
    }
}

```

**5. Custom Endpoint**

```csharp
public class CheckProductStock : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/{id}/stock", async ([FromRoute] int id) =>
        {
            return Results.Ok($"Checking stock for product {id}");
        })
        .WithTags("prueba");
    }
}
```

**6. Program.cs**

```csharp
using FastSharp.Modules;
using FastSharp.Modules.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseInMemoryDatabase("fastsharp-demo"));

builder.Services.AddFastSharpEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapFastSharpEndpoints();
app.MapOpenApi();
app.Run();
```

Run the project and open `/openapi/v1.json` — you'll see the generated CRUD endpoints for `/api/products/alternative` plus any custom endpoints you include in the module.


## What does `AddCRUD` generate?

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/products/alternative` | Full list (add `?page=1&pageSize=10` for paginated results) |
| `GET` | `/api/products/alternative/{id}` | Get by ID |
| `POST` | `/api/products/alternative` | Create |
| `PUT` | `/api/products/alternative/{id}` | Update |
| `DELETE` | `/api/products/alternative/{id}` | Delete |

Paths use the module prefix from `ConfigureModule` (these examples use `/api`) plus the `AddCRUD` route prefix (here, `/products/alternative`). **Convention:** pass a leading slash on every path you own (`ConfigureModule`, `AddCRUD`, and custom `MapGet` / `MapPost` templates) so routes stay consistent across modules and the library.

---

## 🧠 Usage Modes

FastSharp can be used in different ways depending on your needs:

1. CRUD-only (fastest)
```csharp
AddCRUD<Product, int>("/products");
```
2. CRUD + custom endpoints
```csharp
AddCRUD<Product, int>("/products");
Include<CheckProductStock>();
```

3. Custom endpoints only (no persistence required)
You can create modules without relying on EF Core and define only IEndpoint implementations.

---

## 🆚 How FastSharp differs from FastEndpoints

FastEndpoints focuses on building endpoints with a structured, opinionated approach.

FastSharp focuses on organizing APIs by domain:

- **Modules (domains) as first-class units**  
- **Endpoints (`IEndpoint`) as implementations inside a module**  
- **Optional CRUD generation** for common cases  
- **Closer to Minimal APIs**, with less framework overhead  

**When to choose each:**

- Choose **FastEndpoints** if you want a more opinionated endpoint-centric framework with built-in pipeline features.  
- Choose **FastSharp** if you prefer explicit modular architecture with lightweight abstractions and domain-oriented organization.

---

## Configuration

**Disable specific endpoints**

```csharp
AddCRUD<Product, int>("/products", crud =>
{
    crud.DisableEndpoint(GenericEndpoint.GetList);
});
```

**Use DTOs**

```csharp
AddCRUD<Product, int>("/products", crud =>
{
    crud.Update<ProductDto>();
    // Or apply DTOs to all endpoints:
    // crud.ConfigureAll<ProductDto>();
});
```

**Add metadata for OpenAPI**

```csharp
AddCRUD<Product, int>("/products", crud =>
{
    crud.Get(endpoint =>
        endpoint.WithDescription("Get a product by its unique identifier"));
});
```

**Validate custom endpoint requests with FluentValidation**

Define your validator and apply `WithValidation<T>()` to the route handler.

```csharp
using FastSharp.Modules.Core;
using FluentValidation;

public record UpdateProductStock(int Id, int Quantity);

public sealed class UpdateProductStockValidator : AbstractValidator<UpdateProductStock>
{
    public UpdateProductStockValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Quantity).NotEqual(0);
    }
}

public sealed class UpdateProductsStock : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapPost("/products/update-stock", (UpdateProductStock request) => Results.NoContent())
            .WithValidation<UpdateProductStock>();
    }
}
```

If no `IValidator<T>` is defined for the request type, the validation filter does nothing and the endpoint continues normally. See [Validation with FluentValidation](docs/validation.md).

**Add custom endpoints to the same module**

```csharp
public ProductsModule()
{
    ConfigureModule("/api", module => module.WithTags("Products"));
    AddCRUD<Product, int>("/products");

    Include<CheckProductStock>();
}
```

```csharp
public class CheckProductStock : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapGet("/{id}/stock", async ([FromRoute] int id) =>
        {
            return Results.Ok($"Checking stock for product {id}");
        })
        .WithTags("prueba");
    }
}
```

Custom `IEndpoint` types are mapped on the **module route group** (the `ConfigureModule` prefix), not nested under each `AddCRUD` prefix. With `/api` as the module prefix, `MapGet("/{id}/stock", ...)` becomes **`GET /api/{id}/stock`**, alongside **`GET /api/products`**, **`GET /api/products/{id}`**, etc. They still share group-level OpenAPI metadata from `ConfigureModule`.

---

> **Module discovery:** With no arguments, `AddFastSharpEndpoints()` and `MapFastSharpEndpoints()` scan the **calling assembly** (typically the project that contains `Program.cs`). If your modules live in another class library, pass that assembly explicitly. See [Assembly scanning](docs/assembly-scanning.md).

> **OpenAPI UI:** The snippet above exposes the OpenAPI document only. For Swagger UI in Development (like the repo sample), add `Swashbuckle` or your preferred UI and call `MapOpenApi` / UI middleware where appropriate.

---

## Architecture

FastSharp is built on **Modular Slices** — group your logic by domain, not by technical layers.

```
YourProject/
└── Modules/
    ├── Products/
    │   ├── ProductsModule.cs
    │   ├── CheckProductStock.cs
    │   └── ProductDto.cs
    └── Orders/
        ├── OrdersModule.cs
        └── OrderDto.cs
```

Each module is a self-contained unit: its routes, its DTOs, its custom endpoints. At startup, FastSharp registers every concrete `IFastModule` and `IEndpoint` type found in the assemblies you pass to `AddFastSharpEndpoints` / `MapFastSharpEndpoints` (default: the calling assembly only). There is no manual “register this module” list beyond that scan.

---

## Requirements

- .NET 10 or higher
- Entity Framework Core
- A registered `DbContext` in the dependency container
- Entities used with the **parameterless** `AddCRUD<TEntity, TKey>(...)` overload must implement `IModel<TId>` (or use the overload that takes an **id selector** expression for plain POCOs)
- Modules inheriting from `Module<TDbContext>`

---

## Docs

- [Modular architecture](docs/architecture.md)
- [Customization](docs/customization.md)
- [Validation with FluentValidation](docs/validation.md)
- [Assembly scanning](docs/assembly-scanning.md)
- [Roadmap](docs/roadmap.md)
- [How to FastSharp](docs/how-to-fastsharp.md)

---

## Minimal APIs, EF Core, and Mapster

FastSharp registers routes using the same building blocks as **ASP.NET Core Minimal APIs** (`MapGet`, `MapGroup`, route handlers, OpenAPI metadata, etc.). You do not need to be an expert to use the built-in CRUD conventions, but anything beyond that (custom `IEndpoint` handlers, policies, filters, or fine-grained OpenAPI) is easier if you already know how Minimal APIs work.

The generic CRUD endpoints run on **Entity Framework Core**: they use your `DbContext`, `DbSet<T>`, LINQ queries, and `SaveChangesAsync` under the hood. Understanding EF Core basics (configuring the context, change tracking, relationships, and migrations in real apps) helps when your entities are more than simple tables.

When you use DTOs (`ConfigureAll`, per-endpoint generic types, etc.), FastSharp uses **Mapster** to map between entities and DTOs (for example `Adapt<T>()`). Customizing those mappings (flattening, ignoring members, global settings) follows Mapster’s configuration model.

- [Minimal APIs overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) — Microsoft Learn  
- [Entity Framework Core documentation](https://learn.microsoft.com/en-us/ef/core/) — Microsoft Learn  
- [Mapster wiki](https://github.com/MapsterMapper/Mapster/wiki) — GitHub

---

## License

MIT
