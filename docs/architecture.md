# Arquitectura modular / Modular architecture

[Volver al README / Back to README](../README.md)

## Índice / Index

- [Filosofía del diseño / Design philosophy](#filosofía-del-diseño--design-philosophy)
- [¿Qué es un módulo? / What is a module?](#qué-es-un-módulo--what-is-a-module)
- [Diferencias con Controllers tradicionales / Differences with traditional Controllers](#diferencias-con-controllers-tradicionales--differences-with-traditional-controllers)
- [Relación con Vertical Slice Architecture / Relationship with Vertical Slice Architecture](#relación-con-vertical-slice-architecture--relationship-with-vertical-slice-architecture)
- [Ejemplo: múltiples CRUDs y endpoints personalizados](#ejemplo-múltiples-cruds-y-endpoints-personalizados--example-multiple-cruds-and-custom-endpoints)

---

## Filosofía del diseño / Design philosophy

**ES**:  
FastSharp no organiza el backend por capas técnicas tradicionales (Controllers, Services, Repositories), sino por **módulos de dominio**.  
Cada módulo representa una capacidad funcional del sistema y actúa como una unidad de composición y configuración.

El objetivo es:
- Reducir fricción al crear APIs
- Mantener alta cohesión por dominio
- Facilitar la escalabilidad del proyecto y del equipo

**EN**:  
FastSharp does not organize the backend around traditional technical layers (Controllers, Services, Repositories), but around **domain modules**.  
Each module represents a functional capability of the system and acts as a composition and configuration unit.

The goal is:
- Reduce friction when creating APIs
- Keep high cohesion by domain
- Make project and team scalability easier

---

## ¿Qué es un módulo? / What is a module?

**ES**:  
Un módulo representa un dominio o funcionalidad específica del sistema.  
Un módulo puede agrupar **múltiples CRUDs** y **endpoints personalizados** bajo un mismo contexto.

Un módulo es responsable de:
- Registrar endpoints
- Declarar CRUDs genéricos
- Configurar metadata HTTP (tags, descriptions, routes)
- Actuar como frontera de dominio

Un módulo **no debería**:
- Contener lógica de negocio compleja
- Implementar queries directamente
- Acceder o acoplarse a otros módulos

**EN**:  
A module represents a specific domain or feature of the system.  
A module can group **multiple CRUDs** and **custom endpoints** under a single context.

A module is responsible for:
- Registering endpoints
- Declaring generic CRUDs
- Configuring HTTP metadata (tags, descriptions, routes)
- Acting as a domain boundary

A module **should not**:
- Contain complex business logic
- Implement queries directly
- Access or couple to other modules

---

## Diferencias con Controllers tradicionales / Differences with traditional Controllers

**ES**:  
Un `Module` **no es** un Controller MVC tradicional.

Principales diferencias:
- No hereda de `ControllerBase`
- No maneja requests directamente
- No define acciones HTTP
- Su responsabilidad es **componer y configurar endpoints**

Esto evita controllers sobredimensionados y promueve una organización basada en dominio.

**EN**:  
A `Module` is **not** a traditional MVC controller.

Main differences:
- It does not inherit from `ControllerBase`
- It does not handle requests directly
- It does not define HTTP actions
- Its responsibility is **composing and configuring endpoints**

This avoids oversized controllers and promotes a domain-based organization.

---

## Relación con Vertical Slice Architecture / Relationship with Vertical Slice Architecture

**ES**:  
FastSharp se inspira en los principios de **Vertical Slice Architecture**, pero no impone un slice por endpoint.

En lugar de eso:
- Agrupa múltiples slices relacionados dentro de un mismo módulo de dominio
- Permite que CRUDs y endpoints personalizados coexistan
- Mantiene la cohesión sin fragmentar excesivamente el código

Esto ofrece un equilibrio entre granularidad y simplicidad.

**EN**:  
FastSharp is inspired by **Vertical Slice Architecture**, but it does not enforce one slice per endpoint.

Instead:
- It groups multiple related slices within a single domain module
- It allows CRUDs and custom endpoints to coexist
- It keeps cohesion without excessively fragmenting the code

This offers a balance between granularity and simplicity.

---

## Ejemplo: múltiples CRUDs y endpoints personalizados / Example: multiple CRUDs and custom endpoints

```csharp
// YourProject/Modules/Inventory/InventoryModule.cs
using FastSharp.Modules;
using FastSharp.Modules.Configuration;
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

## Representación visual / Visual representation

InventoryModule
 ├─ CRUD: Products
 ├─ CRUD: Categories
 ├─ Endpoint: CheckStock
 └─ Endpoint: AdjustInventory