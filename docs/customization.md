# Personalizacion / Customization

[Volver al README / Back to README](../README.md)

## Indice / Index

- [Arquitectura modular / Modular architecture](architecture.md)
- [Uso basico / Basic usage](basic-usage.md)
- [Endpoints personalizados / Custom endpoints](custom-endpoints.md)

**ES**: La personalizacion de los endpoints CRUD se realiza en el constructor de tu modulo a traves del metodo `AddCRUD`. Este metodo te da acceso a un objeto de opciones (`CrudOptions<TModel, TId>`), que te permite modificar o deshabilitar los endpoints genericos.

**EN**: Customization of CRUD endpoints is done in your module's constructor via the `AddCRUD` method. This method gives you access to an options object (`CrudOptions<TModel, TId>`), allowing you to modify or disable the generic endpoints.

**ES**: Para configurar el grupo base del modulo (aplicando opciones a todos los endpoints del grupo), usa `ConfigureGroup`. Esto te permite agregar metadata o politicas a nivel de grupo.

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

        AddCRUD<Product, int>(options =>
        {
            // Example 1: Disable an endpoint
            options.DisableEndpoint(GenericEndpoint.GetList);

            // Example 2: Add OpenAPI metadata to an endpoint
            options.ConfigureEndpoint(GenericEndpoint.Delete, endpoint =>
            {
                endpoint.WithDescription("Deletes a product permanently.");
            });
        }, "/products");
    }
}
```

**ES**: El parametro `endpoint` dentro de `options.ConfigureEndpoint` es un `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (o similar, segun la version de .NET). Esto te permite acceder a los metodos de extension de Minimal APIs para configuracion granular (ej. `WithOpenApi`, `RequireAuthorization`, `Accepts`, `Produces`).

**EN**: The `endpoint` parameter within `options.ConfigureEndpoint` is a `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (or similar, depending on the .NET version). This grants access to Minimal APIs extension methods for granular configuration (e.g., `WithOpenApi`, `RequireAuthorization`, `Accepts`, `Produces`).
