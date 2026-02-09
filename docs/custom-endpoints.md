# Endpoints personalizados / Custom endpoints

[Volver al README / Back to README](../README.md)

## Indice / Index

- [Arquitectura modular / Modular architecture](architecture.md)
- [Uso basico / Basic usage](basic-usage.md)
- [Personalizacion / Customization](customization.md)

**ES**: Ademas de los endpoints CRUD, puedes crear tus propios endpoints implementando `IFastEndpoint`. Estos deben ser registrados en un modulo para ser mapeados. Los endpoints personalizados se anidan bajo la ruta del modulo.

**EN**: Besides CRUD endpoints, you can create your own by implementing `IFastEndpoint`. These must be registered within a module to be mapped. Custom endpoints are nested under the module's route.

## 1) Definir el endpoint / Define the endpoint

```csharp
// YourProject/Slices/Products/Endpoints/CheckStock.cs
using FastSharp.Controllers;
using Microsoft.AspNetCore.Mvc;

public class CheckStock : IFastEndpoint
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

## 2) Incluir en el modulo / Include in module

**ES**: Usa `Include<T>()` para un endpoint individual o `IncludeNamespace<T>()` para todos los endpoints en un namespace.

**EN**: Use `Include<T>()` for a single endpoint or `IncludeNamespace<T>()` for all endpoints in a namespace.

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

Esto resultara en un nuevo endpoint: `GET /api/products/{id}/stock`.
This will result in a new endpoint: `GET /api/products/{id}/stock`.
