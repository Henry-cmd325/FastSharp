# FastSharp

[![NuGet](https://img.shields.io/nuget/v/FastSharp.Modules?logo=nuget)](https://www.nuget.org/packages/FastSharp.Modules)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
[![Publish NuGet packages](https://github.com/Henry-cmd325/FastSharp/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/Henry-cmd325/FastSharp/actions/workflows/nuget-publish.yml)
[![Stars](https://img.shields.io/github/stars/Henry-cmd325/FastSharp?logo=github&style=flat)](https://github.com/Henry-cmd325/FastSharp)

**ES**: FastSharp es una librería ligera para crear MVPs y APIs en C# y ASP.NET Core (Minimal APIs) con CRUDs y endpoints personalizados basados en convenciones.

**EN**: FastSharp is a lightweight library for building MVPs and APIs in C# and ASP.NET Core (Minimal APIs) with convention-based CRUDs and custom endpoints.

---

## Características / Features

- 🚀 **Zero Boilerplate**: CRUDs completos en una sola línea de código. / Full CRUDs in a single line of code.
- 🏗 **Modular Architecture**: Organiza tu código por dominios, no por capas técnicas. / Organize your code by domains, not by technical layers.
- 🔍 **Auto-Discovery**: Escaneo automático de endpoints por Namespace. / Automatic endpoint scanning by namespace.
- 📄 **OpenAPI Ready**: Integración nativa con Swagger y metadatos de endpoints. / Native integration with Swagger and endpoint metadata.
- ⚡ **High Performance**: Construido sobre Minimal APIs y Entity Framework Core. / Built on Minimal APIs and Entity Framework Core.

---

## Instalación / Installation

```bash
dotnet add package FastSharp.Modules
dotnet add package FastSharp.Models
```

> Nota / Note: Este repositorio contiene dos librerías: `FastSharp.Modules` (core) y `FastSharp.Models` (interfaces de modelos).

---

## Requisitos / Requirements

**ES**
- .NET 10 (o superior, según el `TargetFramework` del paquete)
- Entity Framework Core
- Tu aplicación debe registrar un `DbContext` en el contenedor de dependencias.
- Tus modelos deben implementar `IModel<TId>`.
- Tus módulos deben heredar de `Module<TDbContext>`.

**EN**
- .NET 10 (or higher, based on the package `TargetFramework`)
- Entity Framework Core
- Your app must register a `DbContext` in the dependency container.
- Your models must implement `IModel<TId>`.
- Your modules must inherit from `Module<TDbContext>`.

---

## Quick start / Inicio rápido

```csharp
// YourProject/Slices/Products/ProductsModule.cs
using FastSharp.Modules;
using FastSharp.Modules.Configuration;
using YourProject.Data;
using YourProject.Models;

public class ProductsModule : Module<ApiDbContext>
{
    public ProductsModule()
    {
        // Configure the endpoint group for this module (also applies to CRUDs) this method is optional but allows you to set metadata or policies at the group level.
        ConfigureGroup(opt =>
            {
                opt.WithTags("Productos")
                .WithDescription("Endpoints for managing products in the inventory");
            });
        
        AddCRUD<Product, int>("/products", opt =>
        {
            opt.DisableEndpoint(GenericEndpoint.GetList);
            opt.ConfigureAll(endpoint => endpoint.WithTags("Products"));
        });

        IncludeNamespace<CheckProductStock>();
    }
}
```

```csharp
// Program.cs
using FastSharp.Modules;
using Microsoft.EntityFrameworkCore;
using YourProject.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<YourDbContext>(opt =>
    opt.UseInMemoryDatabase("MyDatabase"));

// Add FastSharp to the dependency container
builder.Services.AddFastSharpEndpoints();

builder.Services.AddOpenApi();

var app = builder.Build();

// Map FastSharp endpoints
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
- [Uso básico / Basic usage](docs/basic-usage.md)
- [Personalización / Customization](docs/customization.md)
- [Descubrimiento por ensamblados / Assembly scanning](docs/assembly-scanning.md)
- [Roadmap](docs/roadmap.md)

---

## License / Licencia

MIT
