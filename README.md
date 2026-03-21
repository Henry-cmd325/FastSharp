# FastSharp

[![NuGet](https://img.shields.io/nuget/v/FastSharp.Modules?logo=nuget)](https://www.nuget.org/packages/FastSharp.Modules)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
[![Publish NuGet packages](https://github.com/Henry-cmd325/FastSharp/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/Henry-cmd325/FastSharp/actions/workflows/nuget-publish.yml)
[![Stars](https://img.shields.io/github/stars/Henry-cmd325/FastSharp?logo=github&style=flat)](https://github.com/Henry-cmd325/FastSharp)

**FastSharp** is a lightweight metaframework for building APIs in C# and ASP.NET Core (Minimal APIs). Full CRUDs and custom endpoints, organized by domain modules, in a single line of code.

---

## Why FastSharp?

In ASP.NET Core Minimal APIs, even a simple CRUD for one entity means writing the same boilerplate over and over. FastSharp eliminates that:

```csharp
// This single line generates 6 REST endpoints backed by EF Core
AddCRUD<Product, int>("products");
```

No controllers. No repetition. Just modules organized by domain.

---

## Installation

```bash
dotnet add package FastSharp.Modules
dotnet add package FastSharp.Models
```

> `FastSharp.Modules` is the core. `FastSharp.Models` contains only the model interfaces — add it to projects that don't need the full core.

---

## Quick Start

The minimum setup requires 4 files. This example uses an in-memory database so you can run it immediately.

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

public class Product : IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**3. Your DbContext**

```csharp
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
}
```

**4. Your module**

```csharp
// Modules/Products/ProductsModule.cs
using FastSharp.Modules;

public class ProductsModule : Module<AppDbContext>
{
    public ProductsModule()
    {
        ConfigureModule("api/", module => module.WithTags("Products"));
        AddCRUD<Product, int>("products");
    }
}
```

**5. Program.cs**

```csharp
using FastSharp.Modules;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("fastsharp-demo"));

builder.Services.AddFastSharpEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapFastSharpEndpoints();
app.MapOpenApi();
app.Run();
```

Run the project and open `/openapi/v1.json` — you'll see all 6 endpoints ready to use.

---

## What does `AddCRUD` generate?

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/products/paged` | Paginated list |
| `GET` | `/api/products` | Full list |
| `GET` | `/api/products/{id}` | Get by ID |
| `POST` | `/api/products` | Create |
| `PUT` | `/api/products/{id}` | Update |
| `DELETE` | `/api/products/{id}` | Delete |

---

## Configuration

**Disable specific endpoints**

```csharp
AddCRUD<Product, int>("products", crud =>
{
    crud.DisableEndpoint(GenericEndpoint.GetList);
});
```

**Use DTOs**

```csharp
AddCRUD<Product, int>("products", crud =>
{
    crud.Update<ProductDto>();
    // Or apply DTOs to all endpoints:
    // crud.ConfigureAll<ProductDto>();
});
```

**Add metadata for OpenAPI**

```csharp
AddCRUD<Product, int>("products", crud =>
{
    crud.Get(endpoint =>
        endpoint.WithDescription("Get a product by its unique identifier"));
});
```

**Add custom endpoints to the same module**

```csharp
public ProductsModule()
{
    ConfigureModule("api/", module => module.WithTags("Products"));
    AddCRUD<Product, int>("products");

    // Custom endpoints inherit the module's group configuration
    Include<CheckProductStock>();
}
```

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

Each module is a self-contained unit: its routes, its DTOs, its custom endpoints. FastSharp auto-discovers all modules in your assembly — no manual registration needed.

---

## Requirements

- .NET 10 or higher
- Entity Framework Core
- A registered `DbContext` in the dependency container
- Models implementing `IModel<TId>`
- Modules inheriting from `Module<TDbContext>`

---

## Docs

- [Modular architecture](docs/architecture.md)
- [Customization](docs/customization.md)
- [Assembly scanning](docs/assembly-scanning.md)
- [Roadmap](docs/roadmap.md)

---

## License

MIT
