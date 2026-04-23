# 🧠 How to FastSharp

This guide shows the recommended ways to structure a FastSharp project.

FastSharp is intentionally flexible: you can start with a very simple setup and evolve into a more modular architecture as your application grows.

There is no single required structure. Instead, FastSharp supports a progression:

1. **Single Project** for simple apps and MVPs
2. **Organized Monolith** for growing applications
3. **Modular Assemblies** for stronger separation and reuse

If you are just getting started, begin simple and only add complexity when your project actually needs it.

---

## When should I use each approach?

Choose the structure based on your current stage, not your ideal future architecture.

- Start with **Single Project** if you want speed and minimal friction
- Move to **Organized Monolith** when the codebase needs clearer boundaries
- Use **Modular Assemblies** when domains need stronger separation or reuse

---

## 🟢 1. Single Project (MVP / Simple apps)

Everything lives in a single project:

- Modules
- Endpoints (`IEndpoint`)
- DbContext
- DTOs

This is the simplest way to get started and matches the Quick Start example.

**Use this when:**

* Building prototypes or MVPs
* Creating small applications
* Learning FastSharp

```text
MyApi/
├── Modules/
│   └── Products/
│       ├── ProductsModule.cs
│       ├── Endpoints/
│       │   └── CheckProductStock.cs
│       └── DTOs/
│           └── ProductDto.cs
├── Data/
│   └── ApiDbContext.cs
└── Program.cs
```

---

### 🟡 2. Organized Monolith (Layered Modules)

You keep a single project but introduce better organization by grouping modules and related code.

For example, placing modules and endpoints inside an `Infrastructure` (or similar) folder:

```text
MyApi/
├── Infrastructure/
│   └── Modules/
│       ├── Products/
│       └── Orders/
├── Application/
├── Domain/
└── Program.cs
```

**Use this when:**

* Your application is growing
* You want clearer separation without multiple projects
* You follow a layered or clean architecture approach

---

### 🔵 3. Modular Assemblies (Advanced)

Each module (or group of modules) lives in its own class library (assembly).
Your API composes itself by loading the assemblies you register.

```text
Solution/
├── Api/
│   └── Program.cs
├── Modules.Products/
├── Modules.Orders/
└── Modules.Inventory/
```

```csharp
builder.Services.AddFastSharpEndpoints(typeof(ProductsModule).Assembly);
app.MapFastSharpEndpoints(typeof(ProductsModule).Assembly);
```

**Use this when:**

* Building modular monoliths
* Sharing modules across multiple APIs
* You want strong separation between domains

---

## 🚀 Recommended Approach

Start simple and scale as needed:

1. Begin with a **Single Project**
2. Organize into **Modules** as your app grows
3. Move modules into **separate assemblies** when needed

FastSharp is designed so you can evolve your architecture without changing how modules or endpoints are written.

---

## Looking for templates?

Templates may be introduced later to make starting easier, but this guide explains the underlying structure and when to use each approach.