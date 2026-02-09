# FastSharp

**ES**: FastSharp es una libreria ligera para crear MVPs y APIs en C# y ASP.NET Core (Minimal APIs) con CRUDs y endpoints personalizados basados en convenciones.

**EN**: FastSharp is a lightweight library for building MVPs and APIs in C# and ASP.NET Core (Minimal APIs) with convention-based CRUDs and custom endpoints.

---

## Instalacion / Installation

```bash
dotnet add package FastSharp.Controllers
dotnet add package FastSharp.Models
```

> Nota / Note: Este repositorio contiene dos librerias: `FastSharp.Controllers` (core) y `FastSharp.Models` (interfaces de modelos).

---

## Requisitos / Requirements

**ES**
- .NET 10 (o superior, segun el `TargetFramework` del paquete)
- Entity Framework Core
- Tu aplicacion debe registrar un `DbContext` en el contenedor de dependencias.
- Tus modelos deben implementar `IModel<TId>`.
- Tus modulos deben heredar de `Module<TDbContext>`.

**EN**
- .NET 10 (or higher, based on the package `TargetFramework`)
- Entity Framework Core
- Your app must register a `DbContext` in the dependency container.
- Your models must implement `IModel<TId>`.
- Your modules must inherit from `Module<TDbContext>`.

---

## Quick start / Inicio rapido

```csharp
// YourProject/Slices/Products/ProductsModule.cs
using FastSharp.Controllers;
using FastSharp.Controllers.Configuration;
using YourProject.Data;
using YourProject.Models;

public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        ConfigureGroup(opt =>
        {
            opt.WithTags("Products")
               .WithDescription("Endpoints for managing products");
        });

        AddCRUD<Product, int>(opt => { }, "/products");
    }
}
```

```csharp
// Program.cs
using FastSharp.Controllers;
using Microsoft.EntityFrameworkCore;
using YourProject.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<YourDbContext>(opt =>
    opt.UseInMemoryDatabase("MyDatabase"));
builder.Services.AddFastSharpEndpoints();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapFastSharpEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();
```

---

## Docs

- [Arquitectura modular / Modular architecture](docs/architecture.md)
- [Uso basico / Basic usage](docs/basic-usage.md)
- [Personalizacion / Customization](docs/customization.md)
- [Endpoints personalizados / Custom endpoints](docs/custom-endpoints.md)
- [Descubrimiento por ensamblados / Assembly scanning](docs/assembly-scanning.md)
- [Roadmap](docs/roadmap.md)

---

## License / Licencia

MIT
