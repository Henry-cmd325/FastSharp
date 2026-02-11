# Uso básico / Basic usage

[Volver al README / Back to README](../README.md)

## Índice / Index

- [Arquitectura modular / Modular architecture](architecture.md)
- [Personalización / Customization](customization.md)

## 1) Modelo / Model

```csharp
// YourProject/Models/Product.cs
using FastSharp.Models;

public class Product : IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

## 2) DbContext

```csharp
// YourProject/Data/YourDbContext.cs
using Microsoft.EntityFrameworkCore;
using YourProject.Models;

public class YourDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
}
```

## 3) Módulo / Module

```csharp
// YourProject/Slices/Products/ProductsModule.cs
using FastSharp.Modules;
using FastSharp.Modules.Configuration;
using YourProject.Models;
using YourProject.Data;

public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        // Configure the endpoint group for this module (also applies to CRUDs) this method is optional but allows you to set metadata or policies at the group level.
        ConfigureGroup(opt =>
        {
            opt.WithTags("Products")
               .WithDescription("Endpoints for managing products in the inventory");
        });

        AddCRUD<Product, int>("/products", opt =>
        {
            // Example: Disable an endpoint
            opt.DisableEndpoint(GenericEndpoint.GetList)
               // Example: Configure a specific endpoint
               .ConfigureEndpoint(GenericEndpoint.Delete, endpoint =>
                   endpoint.WithDescription("Deletes a product by its unique identifier"));
        });
    }
}
```

## 4) Program.cs / Minimal API setup

```csharp
using FastSharp.Modules;
using Microsoft.EntityFrameworkCore;
using YourProject.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Register your DbContext
builder.Services.AddDbContext<YourDbContext>(opt =>
    opt.UseInMemoryDatabase("MyDatabase"));

// 2. Register FastSharp services and enable OpenAPI
builder.Services.AddFastSharpEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();

// 3. Map FastSharp endpoints and enable OpenAPI UI
app.MapFastSharpEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();
```

---

## Endpoints generados / Generated endpoints

Para un módulo llamado `ProductsModule` configurado con `AddCRUD<Product, int>("/products", ...)`, FastSharp genera la siguiente ruta base: `/api/products`.

For a module named `ProductsModule` configured with `AddCRUD<Product, int>("/products", ...)`, FastSharp generates the following base route: `/api/products`.

- `GET    /api/products` -> Lista todos los productos / Lists all products.
- `GET    /api/products/{id}` -> Obtiene un producto por su ID / Gets a product by ID.
- `POST   /api/products` -> Crea un nuevo producto / Creates a new product.
- `PUT    /api/products/{id}` -> Actualiza un producto existente / Updates an existing product.
- `DELETE /api/products/{id}` -> Elimina un producto / Deletes a product.
