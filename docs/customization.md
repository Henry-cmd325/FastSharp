# Personalización / Customization

[Volver al README / Back to README](../README.md)

## Índice / Index

- [Arquitectura modular / Modular architecture](architecture.md)
- [Uso básico / Basic usage](basic-usage.md)

**ES**: La personalización de los endpoints CRUD se realiza en el constructor de tu módulo a través del método `AddCRUD`. El primer parámetro es la ruta base y el segundo (opcional) te da acceso a un objeto de opciones (`CrudOptions<TModel, TId>`), que te permite modificar o deshabilitar los endpoints genéricos.

**EN**: Customization of CRUD endpoints is done in your module's constructor via the `AddCRUD` method. The first parameter is the base route and the second (optional) provides an options object (`CrudOptions<TModel, TId>`), allowing you to modify or disable the generic endpoints.

**ES**: Para configurar el grupo base del módulo (aplicando opciones a todos los endpoints del grupo), usa `ConfigureGroup`. Esto te permite agregar metadata o políticas a nivel de grupo.

**EN**: To configure the module base group (applying options to all endpoints in the group), use `ConfigureGroup`. This lets you add metadata or policies at the group level.

```csharp
using FastSharp.Modules;
using FastSharp.Modules.Configuration;
using YourProject.Models;
using YourProject.Data;

public class ProductsModule : Module<YourDbContext>
{
    public ProductsModule()
    {
        ConfigureGroup(group =>
        {
            group.WithDescription("Endpoints for managing products in the inventory");
        });

        AddCRUD<Product, int>("/products", options =>
        {
            // Example 1: Disable an endpoint
            options.DisableEndpoint(GenericEndpoint.GetList)
                   // Example 2: Add OpenAPI metadata to an endpoint
                   .ConfigureEndpoint(GenericEndpoint.Delete, endpoint =>
                   {
                       endpoint.WithDescription("Deletes a product permanently.");
                   });
        });
    }
}
```

**ES**: El parámetro `endpoint` dentro de `options.ConfigureEndpoint` es un `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (o similar, según la versión de .NET). Esto te permite acceder a los métodos de extensión de Minimal APIs para configuración granular (ej. `WithOpenApi`, `RequireAuthorization`, `Accepts`, `Produces`).

**EN**: The `endpoint` parameter within `options.ConfigureEndpoint` is a `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (or similar, depending on the .NET version). This grants access to Minimal APIs extension methods for granular configuration (e.g., `WithOpenApi`, `RequireAuthorization`, `Accepts`, `Produces`).

---

# Endpoints personalizados / Custom endpoints

[Volver al README / Back to README](../README.md)

## Índice / Index

- [Arquitectura modular / Modular architecture](architecture.md)
- [Uso básico / Basic usage](basic-usage.md)

**ES**: Además de los endpoints CRUD, puedes crear tus propios endpoints implementando `IFastEndpoint`. Estos deben ser registrados en un módulo para ser mapeados. Los endpoints personalizados se anidan bajo la ruta del módulo.

**EN**: Besides CRUD endpoints, you can create your own by implementing `IFastEndpoint`. These must be registered within a module to be mapped. Custom endpoints are nested under the module's route.

## 1) Definir el endpoint / Define the endpoint

```csharp
// YourProject/Slices/Products/Endpoints/CheckStock.cs
using FastSharp.Modules;
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

## 2) Incluir en el módulo / Include in module

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

Esto resultará en un nuevo endpoint: `GET /api/products/{id}/stock`.
This will result in a new endpoint: `GET /api/products/{id}/stock`.
