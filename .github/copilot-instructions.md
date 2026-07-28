# Copilot Instructions

## Project Guidelines
- Frame FastSharp as modules first, endpoints as implementations, and CRUD as an optional shortcut.
- All code comments and XML documentation in this project should be written in English.
- When requesting documentation improvements, prioritize `.md` documentation files (docs) over XML documentation in code, unless specified otherwise.
- Keep project-structure guidance prominent. The published `FastSharp.Templates` package provides the `fastsharp-api` template and installs with `dotnet new install FastSharp.Templates`.

## Minimal API Convention Guidelines
- Use the module-level `IEndpointConventionBuilder` exposed by `ConfigureModule(...)` for shared metadata, authorization, filters, and other group conventions.
- Use the CRUD endpoint-level `RouteHandlerBuilder` for operation-specific native Minimal API chaining such as `RequireAuthorization()`.
