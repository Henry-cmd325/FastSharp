# ADR: Assembly Registry Lifecycle

## Status

Accepted

## Context

FastSharp currently resolves source-generated endpoint metadata through a process-wide static registry store:

- generated code registers an assembly registry through a `ModuleInitializer`
- `AddFastSharpEndpoints(...)` and `MapFastSharpEndpoints(...)` resolve metadata from that store
- consumers still choose explicitly which assemblies each application should use

This design is important to evaluate because FastSharp is intended to support applications that split modules and endpoints across multiple assemblies, including microservice solutions where each service chooses a different set of assemblies.

## Decision

Keep the current static registry design for now.

## Why this is acceptable now

- FastSharp's current target scenario is the normal ASP.NET Core model: one service host per process.
- In that scenario, each microservice has its own process and therefore its own static registry state.
- Consumers already control which assemblies are used by each service through `AddFastSharpEndpoints(...)` and `MapFastSharpEndpoints(...)`.
- Dynamic assembly loading is not a current project goal.
- Replacing the registry mechanism now would add architectural churn without solving a concrete problem seen in the main usage model.

## Risks we accept

- The registry store is process-global state rather than host-local state.
- The internal design is less explicit than the public API suggests because runtime resolution depends on generated side effects.
- If FastSharp is later used with multiple hosts in the same process, registry state will be shared across those hosts.
- Some advanced test or tooling scenarios may be harder to isolate because metadata is not owned only by the current `ServiceProvider`.

## When to revisit this decision

Revisit the registry design if any of these become important:

- running multiple FastSharp hosts in the same process
- integration tests that show cross-host contamination or hard-to-explain registry state
- tooling that needs host-local registry inspection or stronger isolation guarantees
- a future FastSharp design goal where the provided `Assembly` must also be the direct internal source of truth for metadata resolution

## Preferred future direction

If the current design stops being sufficient, prefer this direction:

- resolve registry metadata directly from the target `Assembly`
- make the contract between generator and runtime explicit
- keep any registry cache as an implementation detail only, not as the primary source of truth

## Summary

The current registry design is acceptable for FastSharp's current microservice-oriented usage model, but it is a known architectural limitation rather than an ideal end state.
