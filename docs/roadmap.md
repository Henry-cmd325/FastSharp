# Roadmap

[Back to README](../README.md)

## Index

- [Modular architecture](architecture.md)

# FastSharp 1.0 Minimum Viable Product (MVP)
- [x] DTOs in generated CRUD endpoints
- [x] FluentValidation integration for custom endpoints
- [x] Structured logging for module mapping and CRUD operations
- [ ] Basic `dotnet new` API template (`fastsharp-api`, issue #29; local pack exists, remaining acceptance criteria pending)
- [ ] Filtering (optional)

# 🚀 FastSharp Framework Roadmap

FastSharp is a lightweight .NET 10 library for domain modules and endpoint implementations. EF Core CRUD generation is available when a module needs it, but is optional.

---

## 🏗️ Phase 1: Modular Core
*Goal: Keep modules as route-group contracts and endpoints as independent implementations.*

- [x] Module and `IEndpoint` discovery and mapping
- [x] Custom-endpoint-only `Module` support
- [x] EF Core-backed `Module<TDbContext>` CRUD support
- [ ] Per-module dependency registration for reusable module packages

## 🛠️ Phase 2: DX Refinement
*Goal: Improve diagnostics, conventions, and consumer guidance.*

- [ ] **Internal Scoping Pattern**:
    - Standardize the use of `internal` modifiers for services to prevent cross-module leakage.
    - Document the "Public API / Internal Implementation" pattern for module creators.
- [ ] **Fluent Endpoint Mapping**:
    - Enhance `MapFastSharpEndpoints` to support automatic Metadata generation (Tags, Security, and Validation) based on module context.
- [ ] **Error Messaging & Diagnostics**:
    - Implement clear startup diagnostics to alert developers if a module dependency is missing.

## 🖥️ Phase 3: GenUIModule (Automated Admin Panel)
*Goal: Provide immediate visual value by generating a Blazor-based backoffice from API metadata.*

- [ ] **Metadata Extraction Engine**:
    - Create a reflection utility to "read" `IEndpoint` definitions and their associated DTOs/Records.
- [ ] **Dynamic Blazor Components**:
    - Develop generic UI components (DataGrids, Auto-Forms) that adapt to the discovered metadata.
- [ ] **Zero-Code Admin Dashboard**:
    - Launch `/admin` dashboard that automatically builds CRUD interfaces for any registered module.
    - Integrate with FluentValidation to provide real-time client-side feedback in generated forms.

## 📦 Phase 4: Packaging & Ecosystem
*Goal: Prepare FastSharp for distribution and community adoption.*

- [ ] **NuGet Modularization**:
- [ ] **Basic API template**: A local `FastSharp.Templates` pack provides `dotnet new fastsharp-api`; issue #29 remains in progress (public publishing, `Program.cs` size, and `Api.http` are pending).
- [ ] **Clean Architecture sample**: Add a reference sample that places FastSharp in the API layer (issue #30).
- [ ] **Clean Architecture template**: Add `dotnet new fastsharp-clean` after the sample is complete (issue #31; blocked by #30).
- [ ] **Real-world sample**: Build a reference implementation beyond the basic template.

---

> **Vision**: To make .NET development as agile as modern frontend frameworks, without sacrificing the robustness of the C# ecosystem.
