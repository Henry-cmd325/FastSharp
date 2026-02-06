# FastSharp

**ES**: FastSharp es una librería ligera diseñada para potenciar el desarrollo rápido de Minimum Viable Products (MVPs) y APIs en C# y ASP.NET Core (Minimal APIs). Combina la potencia y seguridad del sistema de tipos de C# con una forma ágil de crear endpoints CRUD y personalizados, utilizando Entity Framework Core y un diseño basado en convenciones. Ideal para quienes buscan construir APIs robustas y escalables con la velocidad que exigen los proyectos modernos.

**EN**: FastSharp is a lightweight library designed to empower rapid development of Minimum Viable Products (MVPs) and APIs in C# and ASP.NET Core (Minimal APIs). It combines the power and safety of C#'s type system with an agile approach to creating CRUD and custom endpoints, leveraging Entity Framework Core and a convention-based design. Ideal for those looking to build robust and scalable APIs with the speed demanded by modern projects.

---

## Motivación / Motivation

**ES**: En la industria actual, existe una creciente demanda por la creación rápida de prototipos y Minimum Viable Products (MVPs). Aunque C# es un lenguaje potente y seguro gracias a su tipado estático, a menudo no es la primera opción para proyectos que requieren una iteración extremadamente veloz.

`FastSharp` nace con la visión de cambiar esta percepción, demostrando que C# puede ser no solo robusto y performante, sino también increíblemente ágil para el desarrollo de APIs. Nuestro objetivo es ofrecer una herramienta que permita a los desarrolladores construir soluciones backend con la **seguridad y mantenibilidad** que ofrece el tipado fuerte, pero con la **velocidad y simplicidad** necesarias para lanzar MVPs en tiempo récord. Buscamos democratizar el uso de C# para proyectos ágiles, sin comprometer la calidad o la escalabilidad futura.

**EN**: In today's industry, there is a growing demand for rapid prototyping and Minimum Viable Product (MVP) creation. While C# is a powerful and secure language thanks to its static typing, it is often not the first choice for projects requiring extremely fast iteration.

`FastSharp` is born with the vision to change this perception, demonstrating that C# can be not only robust and performant but also incredibly agile for API development. Our goal is to offer a tool that allows developers to build backend solutions with the **safety and maintainability** provided by strong typing, but with the **speed and simplicity** required to launch MVPs in record time. We aim to democratize the use of C# for agile projects, without compromising quality or future scalability.

---

## Instalación / Installation

**ES/EN (NuGet)**

```bash
dotnet add package FastSharp.Controllers
dotnet add package FastSharp.Models
```

> Nota / Note: Este repositorio contiene dos librerías: `FastSharp.Controllers` (core) y `FastSharp.Models` (interfaces de modelos).

---

## Requisitos / Requirements

**ES**
- .NET 10 (o superior, según el `TargetFramework` del paquete)
- Entity Framework Core
- Tu aplicación debe registrar un `DbContext` en el contenedor de dependencias.
- Tus modelos deben implementar `IModel<TId>`.
- Tus controladores deben heredar de `FastController<TDbContext, TModel, TId>`.

**EN**
- .NET 10 (or higher, based on the package `TargetFramework`)
- Entity Framework Core
- Your app must register a `DbContext` in the dependency container.
- Your models must implement `IModel<TId>`.
- Your controllers must inherit from `FastController<TDbContext, TModel, TId>`.

---

## Uso básico / Basic usage

### 1) Modelo / Model

```csharp
// YourProject/Models/Product.cs
using FastSharp.Models;

public class Product : IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

### 2) DbContext

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

### 3) Controlador / Controller

```csharp
// YourProject/Slices/Products/ProductsController.cs
using FastSharp.Controllers;
using YourProject.Models;
using YourProject.Data;

public class ProductsController : FastController<YourDbContext, Product, int> 
{
    public ProductsController()
    {
        // Customization goes here
    }
}
```

### 4) Program.cs / Minimal API setup

```csharp
using FastSharp.Controllers;
using Microsoft.EntityFrameworkCore;
using YourProject.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Register your DbContext
builder.Services.AddDbContext<YourDbContext>(opt => 
    opt.UseInMemoryDatabase("MyDatabase")); // Or your preferred provider

// 2. Register FastSharp controllers and endpoints
builder.Services.AddFastSharpEndpoints();

var app = builder.Build();

// 3. Map FastSharp controllers
app.MapFastSharpEndpoints();

app.Run();
```

---

## Endpoints generados / Generated endpoints

Para un controlador llamado `ProductsController`, FastSharp genera la siguiente ruta base: `/api/products`.

For a controller named `ProductsController`, FastSharp generates the following base route: `/api/products`.

- `GET    /api/products` → Lista todos los productos / Lists all products.
- `GET    /api/products/{id}` → Obtiene un producto por su ID / Gets a product by ID.
- `POST   /api/products` → Crea un nuevo producto / Creates a new product.
- `PUT    /api/products/{id}` → Actualiza un producto existente / Updates an existing product.
- `DELETE /api/products/{id}` → Elimina un producto / Deletes a product.

---

## Personalización / Customization

**ES**: La personalización de los endpoints CRUD se realiza en el constructor de tu controlador a través del método `ConfigureCRUD`. Este método te da acceso a `ControllerOptions`, que te permite modificar o deshabilitar los endpoints genéricos.

**EN**: Customization of CRUD endpoints is done in your controller's constructor via the `ConfigureCRUD` method. This method gives you access to `ControllerOptions`, allowing you to modify or disable the generic endpoints.

```csharp
using FastSharp.Controllers.Configuration;

public class ProductsController : FastController<YourDbContext, Product, int>
{
    public ProductsController()
    {
        ConfigureCRUD(options =>
        {
            // Example 1: Disable an endpoint
            // ES: Desactiva el endpoint para listar todos los productos
            // EN: Disables the endpoint for listing all products
            options.DisableEndpoint(GenericEndpoint.GetList);

            // Example 2: Add OpenAPI metadata to an endpoint
            // ES: Añade una descripción al endpoint de eliminación
            // EN: Adds a description to the delete endpoint
            options.ConfigureEndpoint(GenericEndpoint.Delete, endpoint =>
            {
                endpoint.WithDescription("Deletes a product permanently.");
            });
        });
    }
}
```

**ES**: Es importante destacar que el parámetro `endpoint` dentro de `options.ConfigureEndpoint` es un `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (o similar, dependiendo de la versión de .NET), lo que te permite acceder a todos los métodos de extensión proporcionados por Minimal APIs de ASP.NET Core para configurar el endpoint de manera granular (ej. `WithOpenApi`, `RequireAuthorization`, `Accepts`, `Produces`, etc.), ofreciendo una gran flexibilidad para cada endpoint generado.

**EN**: It's important to note that the `endpoint` parameter within `options.ConfigureEndpoint` is a `Microsoft.AspNetCore.Builder.RouteHandlerBuilder` (or similar, depending on the .NET version). This grants you access to all extension methods provided by ASP.NET Core Minimal APIs for granular endpoint configuration (e.g., `WithOpenApi`, `RequireAuthorization`, `Accepts`, `Produces`, etc.), offering great flexibility for each generated endpoint.
 
---

## Endpoints Personalizados / Custom Endpoints

**ES**: Además de los endpoints CRUD, puedes crear tus propios endpoints implementando `IFastEndpoint`. Estos deben ser registrados en un controlador para ser mapeados. Los endpoints personalizados se anidan bajo la ruta del controlador.

**EN**: Besides CRUD endpoints, you can create your own by implementing `IFastEndpoint`. These must be registered within a controller to be mapped. Custom endpoints are nested under the controller's route.

### 1) Definir el Endpoint / Define the Endpoint

```csharp
// YourProject/Slices/Products/Endpoints/CheckStock.cs
using FastSharp.Controllers;
using Microsoft.AspNetCore.Mvc;

public class CheckStock : IFastEndpoint
{
    // ES: El RouteGroupBuilder se inyecta desde el controlador padre
    // EN: The RouteGroupBuilder is injected from the parent controller
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id}/stock", async ([FromRoute] int id) =>
        {
            // Your logic here...
            return Results.Ok($"Product {id} has 10 units in stock.");
        })
        .WithTags("Stock");
    }
}
```

### 2) Incluir en el Controlador / Include in Controller

**ES**: Usa `Include<T>()` para un endpoint individual o `IncludeNamespace<T>()` para todos los endpoints en un namespace.

**EN**: Use `Include<T>()` for a single endpoint or `IncludeNamespace<T>()` for all endpoints in a namespace.

```csharp
public class ProductsController : FastController<YourDbContext, Product, int> 
{
    public ProductsController()
    {
        // ES: Incluye todos los endpoints del mismo namespace que CheckStock
        // EN: Includes all endpoints in the same namespace as CheckStock
        IncludeNamespace<CheckStock>();
        
        // ES: O incluye un endpoint específico
        // EN: Or include a specific endpoint
        // Include<CheckStock>();
    }
}
```

Esto resultará en un nuevo endpoint: `GET /api/products/{id}/stock`.
This will result in a new endpoint: `GET /api/products/{id}/stock`.

---

## Descubrimiento por ensamblados / Assembly scanning

**ES**: Si tus controladores están en un ensamblado diferente al de `Program.cs`, debes especificarlo.

**EN**: If your controllers live in a different assembly than `Program.cs`, you must specify it.

```csharp
var assemblies = new[] { typeof(ProductsController).Assembly };

builder.Services.AddFastSharpEndpoints(assemblies);
app.MapFastSharpEndpoints(assemblies);
```

---

## Roadmap

- [ ] Validar `id` de ruta vs `Id` del cuerpo en `PUT` / Validate route `id` vs body `Id` on `PUT`
- [ ] Paginación y filtrado (opcional) / Paging and filtering (optional)
- [ ] Metadatos NuGet y publicación CI/CD / NuGet metadata and CI/CD publish

---

## License / Licencia

MIT