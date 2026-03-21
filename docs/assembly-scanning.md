# Assembly scanning

[Back to README](../README.md)

## Index

- [Modular architecture](architecture.md)
- [Customization](customization.md)

If your modules live in a different assembly than `Program.cs`, you must specify it.

```csharp
var assemblies = new[] { typeof(ProductsModule).Assembly };

builder.Services.AddFastSharpEndpoints(assemblies);
app.MapFastSharpEndpoints(assemblies);
```
