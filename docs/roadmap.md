# Roadmap

[Back to README](../README.md)

## Index

- [Modular architecture](architecture.md)

# FastSharp 1.0 Minimum Viable Product (MVP)
- [x] Use DTOs in generic endpoints
- [x] FluentValidation integration for custom endpoints
- [x] Structured logging for module mapping and CRUD operations
- [ ] Filtering (optional)

# 🚀 FastSharp Framework Roadmap

FastSharp is a modular meta-framework built on top of .NET 10, designed to provide a good developer experience. It focuses on Vertical Slice Architecture, automated dependency injection, and rapid UI generation.

---

## 🏗️ Phase 1: The Modular Core (DI & Scanning)
*Goal: Establish the transversal foundation to allow "Plug-and-Play" modules with assembly-level isolation.*

- [ ] **`IFastSharpModule` Contract**: Define the standard interface for module entry points.
    - Implement `RegisterDependencies(IServiceCollection, IConfiguration)`.
- [ ] **Transversal Orchestrator**:
    - Prepare `AddFastSharpModules(params Assembly[] assemblies)` to scan and execute module registrations also from NuGet packages.

## 🛠️ Phase 2: DX Refinement & Scoping
*Goal: Polish the Developer Experience and enforce clean architecture patterns.*

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
- [ ] **CLI / Scaffolding**:
    - Create a simple template or CLI tool to scaffold new FastSharp modules quickly.
- [ ] **Real-World Sample**:
    - Build a reference implementation (e.g., a Digital Wallet) to demonstrate the power of transversal modules.

---

> **Vision**: To make .NET development as agile as modern frontend frameworks, without sacrificing the robustness of the C# ecosystem.
