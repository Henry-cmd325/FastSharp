---
name: fastsharp
description: "Trigger: /fastsharp module, /fastsharp crud, /fastsharp endpoint, /fastsharp validation. Scaffold FastSharp code using project conventions."
license: Apache-2.0
metadata:
  author: Henry-cmd325
  version: "1.0"
---
## Activation Contract

Use this skill when the user asks to scaffold or adjust FastSharp modules, CRUD registrations, custom endpoints, or validation flows. Keep FastSharp framed as modules plus endpoints first; CRUD is optional.

## Hard Rules

- Use leading slashes for owned route prefixes in `ConfigureModule`, `AddCRUD`, and Minimal API route templates.
- Do not use an `AddCRUD<TEntity, TKey>` overload without an id selector unless `TEntity` implements `IModel<TKey>`; otherwise use the id-selector overload.
- When `AddFastSharpEndpoints` scans explicit assemblies, require that the assembly references `FastSharp.Generators` so a source-generated `IFastSharpAssemblyRegistry` is produced for it.
- Preserve native Minimal API chaining for metadata, authorization, and filters.

## Decision Gates


| Request                 | Scaffold                                                                         |
| ----------------------- | -------------------------------------------------------------------------------- |
| `/fastsharp module`     | A domain module with `ConfigureModule` and optional `Include<TEndpoint>()` calls |
| `/fastsharp crud`       | An `AddCRUD` registration with DTO/configuration hooks when needed               |
| `/fastsharp endpoint`   | An `IEndpoint` implementation mapped on the module route group                   |
| `/fastsharp validation` | FluentValidation validator plus request DTO and endpoint filter wiring           |


## Execution Steps

1. Read the relevant local references before editing.
2. Identify the domain, route prefix, DbContext, entity key type, DTOs, and validation needs.
3. Prefer explicit endpoint classes for behavior; add CRUD only for standard REST operations.
4. Scaffold the smallest useful files and update module registration.
5. Verify route prefixes, id selection, assembly registration, and English comments/docs.

## Output Contract

Return the files changed, generated routes, assumptions made, validation added, and any follow-up required before running the app.

## References

- `skills/fastsharp/references/fastsharp-docs.md`

