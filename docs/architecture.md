# Arquitectura modular / Modular architecture

[Volver al README / Back to README](../README.md)

## Indice / Index

- [Uso basico / Basic usage](basic-usage.md)
- [Personalizacion / Customization](customization.md)
- [Endpoints personalizados / Custom endpoints](custom-endpoints.md)

**ES**: FastSharp promueve una arquitectura modular donde cada modulo representa un dominio o funcionalidad especifica. Un modulo puede agrupar **multiples CRUDs** y **endpoints personalizados** bajo un mismo contexto, lo que facilita la escalabilidad, la colaboracion por equipos y el aislamiento de responsabilidades.

**EN**: FastSharp promotes a modular architecture where each module represents a specific domain or feature. A module can group **multiple CRUDs** and **custom endpoints** under a single context, which improves scalability, team collaboration, and separation of concerns.

## Ejemplo: multiples CRUDs y endpoints personalizados / Example: multiple CRUDs and custom endpoints

```csharp
// YourProject/Slices/Inventory/InventoryModule.cs
using FastSharp.Controllers;
using FastSharp.Controllers.Configuration;
using YourProject.Data;
using YourProject.Models;

public class InventoryModule : Module<YourDbContext>
{
    public InventoryModule()
    {
        ConfigureGroup(group =>
        {
            group.WithTags("Inventory")
                 .WithDescription("Inventory domain endpoints");
        });

        // CRUDs for multiple entities
        AddCRUD<Product, int>(opt => { }, "/products");
        AddCRUD<Category, int>(opt => { }, "/categories");

        // Custom endpoints under the same module
        IncludeNamespace<CheckStock>();
    }
}
```
