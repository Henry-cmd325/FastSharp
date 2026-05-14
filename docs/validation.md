# Validation with FluentValidation

[Back to README](../README.md)

## Index

- [Customization](customization.md)
- [Architecture](architecture.md)

FastSharp includes a small integration with FluentValidation for custom Minimal API endpoints.

Use it when:

- you have a request DTO or record bound in a custom `IEndpoint`
- you want automatic `400 Bad Request` responses with `ValidationProblem` payloads
- you prefer to keep validation close to the endpoint contract

## How it works

FastSharp exposes `WithValidation<T>()` in `FastSharp.Modules.Core`.

When you apply it to a route handler:

- FastSharp looks for an `IValidator<T>` in the current request scope
- if a validator is defined, it validates the bound argument
- if validation fails, the endpoint returns `Results.ValidationProblem(...)`
- if no validator is defined, the filter does nothing and the request continues normally

## 1) Register the validator in DI

```csharp
using FluentValidation;

builder.Services.AddScoped<IValidator<UpdateProductStock>, UpdateProductStockValidator>();
```

## 2) Define the request contract and validator

```csharp
using FluentValidation;

public record UpdateProductStock(int Id, string Name, int Quantity);

public sealed class UpdateProductStockValidator : AbstractValidator<UpdateProductStock>
{
    public UpdateProductStockValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Quantity).NotEqual(0);
    }
}
```

## 3) Apply validation to the endpoint

```csharp
using FastSharp.Modules.Core;
using Microsoft.AspNetCore.Mvc;

public sealed class UpdateProductsStock : IEndpoint
{
    public void Map(RouteGroupBuilder app)
    {
        app.MapPost("/products/update-stock/{id}", async ([FromRoute] int id, [FromBody] UpdateProductStock request) =>
        {
            return Results.NoContent();
        })
        .WithValidation<UpdateProductStock>();
    }
}
```

## Response behavior

For an invalid request, FastSharp returns a standard Minimal APIs validation response:

```json
{
  "errors": {
    "Name": [
      "'Name' must not be empty."
    ]
  }
}
```

with status code `400 Bad Request`.

## Notes

- This integration is intended for custom endpoints built with `IEndpoint`.
- The validated type passed to `WithValidation<T>()` must match the bound request argument type.
- Validation runs through DI, so the validator must be registered in services.
- The feature uses FluentValidation, but FastSharp does not auto-scan or auto-register validators for you.
- If no validator is registered for the request type, `WithValidation<T>()` simply lets the request continue.

## Example in this repository

See:

- `Samples/QuickStart/Modules/Products/Endpoints/UpdateProductsStock.cs`
- `FastSharp.Modules/Core/FastSharpExtensions.cs`
- `FastSharp.Modules/Filters/ValidationFilter.cs`
