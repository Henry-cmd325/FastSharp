# Descubrimiento por ensamblados / Assembly scanning

[Volver al README / Back to README](../README.md)

[Ir al indice de docs / Go to docs index](index.md)

## Indice / Index

- [Uso basico / Basic usage](basic-usage.md)
- [Arquitectura modular / Modular architecture](architecture.md)
- [Personalizacion / Customization](customization.md)

**ES**: Si tus modulos estan en un ensamblado diferente al de `Program.cs`, debes especificarlo.

**EN**: If your modules live in a different assembly than `Program.cs`, you must specify it.

```csharp
var assemblies = new[] { typeof(ProductsModule).Assembly };

builder.Services.AddFastSharpEndpoints(assemblies);
app.MapFastSharpEndpoints(assemblies);
```
