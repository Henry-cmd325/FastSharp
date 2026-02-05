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
dotnet add package FastSharp.Endpoints
dotnet add package FastSharp.Models
```

> Nota / Note: Este repositorio contiene dos librerías: `FastSharp.Endpoints` (core) y `FastSharp.Models` (interfaces de modelos).

---

## Requisitos / Requirements

**ES**
- .NET (según el `TargetFramework` del paquete)
- Entity Framework Core
- Tu aplicación debe registrar un `DbContext` en DI
- Tus modelos deben implementar `IModel<TId>`

**EN**
- .NET (based on the package `TargetFramework`)
- Entity Framework Core
- Your app must register a `DbContext` in DI
- Your models must implement `IModel<TId>`

---

## Uso básico / Basic usage

### 1) Modelo / Model

```csharp
using FastSharp.Models;

public class Product : IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

### 2) Endpoint / Endpoint

```csharp
using FastSharp.Endpoints;

public class ProductsController : FastController<Product, int> { }
```

### 3) Program.cs / Minimal API setup

```csharp
using FastSharp.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ES: registra controllers FastSharp por reflexión
// EN: registers FastSharp controllers via reflection
builder.Services.AddFastSharpEndpoints();

// ES/EN: IMPORTANT - register your EF Core DbContext
// builder.Services.AddDbContext<MyDbContext>(...);

var app = builder.Build();

// ES: mapea endpoints automáticamente
// EN: maps endpoints automáticamente
app.MapFastSharpEndpoints();

app.Run();
```

---

## Endpoints generados / Generated endpoints

Para un modelo `Product` / For `Product`:

- `GET    /api/products` → list all
- `GET    /api/products/{id}` → get by id
- `POST   /api/product` → create
- `PUT    /api/product/{id}` → update
- `DELETE /api/product/{id}` → delete

**ES**
- El nombre base sale de `typeof(TModel).Name.ToLower()`.
- La ruta de listado pluraliza agregando `s`.

**EN**
- Base name comes from `typeof(TModel).Name.ToLower()`.
- List route pluralizes by appending `s`.

---

## Personalización / Customization

Puedes usar los métodos `ConfigureGetList/ConfigureGetById/ConfigurePost/ConfigurePut/ConfigureDelete` para ajustar rutas, OpenAPI metadata, autorizaciones, etc. Estos métodos reciben un `EndpointOptions` para configurar el endpoint.

You can use `ConfigureGetList/ConfigureGetById/ConfigurePost/ConfigurePut/ConfigureDelete` methods to tweak routes, OpenAPI metadata, auth, etc. These methods receive an `EndpointOptions` to configure the endpoint.

```csharp
public class ProductsController : FastController<Product, int>
{
    public ProductsController()
    {
        ConfigureGetList(options =>
        {
            options.Builder = builder => builder.WithSummary("List products");
        });
    }
}
```

### Desactivar rutas generadas / Disable generated routes

```csharp
public class ProductsController : FastController<Product, int>
{
    public ProductsController()
    {
        // ES: desactiva DELETE si no quieres exponerlo
        // EN: disable DELETE if you don't want to expose it
        ConfigureDelete(opt => opt.Active = false);
    }
}
```

---

### Endpoints independientes / Standalone endpoints

Además de los controladores genéricos, puedes crear endpoints sueltos implementando `IFastEndpoint`, como en `Samples/Api/Endpoints/CheckProductStock.cs`:

```csharp
using FastSharp.Controllers;
using Microsoft.AspNetCore.Mvc;

public class CheckProductStock : IFastEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id}/stock", async ([FromRoute] int id) =>
        {
            return Results.Ok($"Checking stock for product {id}");
        })
        .WithTags("prueba");
    }
}
```

Estos endpoints se registran igual que los controladores FastSharp cuando llamas a `app.MapFastSharpEndpoints(...)`.

---

## Descubrimiento por ensamblados / Assembly scanning

Si tus controladores están en otro ensamblado / If your controllers live in another assembly:

```csharp
builder.Services.AddFastSharpEndpoints(new[] { typeof(ProductsController).Assembly });
app.MapFastSharpEndpoints(new[] { typeof(ProductsController).Assembly });
```

---

## Roadmap

- [x] `GET /api/{model}/{id}` (get by id)
- [ ] Better pluralization strategy
- [ ] Validate `id` (route) vs `Id` (body) on `PUT`
- [ ] Paging/filtering (optional)
- [ ] NuGet metadata + CI publish

---

## License / Licencia

MIT