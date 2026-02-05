# FastSharp

**ES**: FastSharp es una librería ligera para crear endpoints CRUD rápidamente en ASP.NET (Minimal APIs) usando Entity Framework Core, basándose en un controlador genérico y convenciones.

**EN**: FastSharp is a lightweight library to quickly create CRUD endpoints in ASP.NET (Minimal APIs) using Entity Framework Core, based on a generic controller and conventions.

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