---
id: solid-review
type: engineering-review
version: 1
status: active
tags:
- dotnet
- solid
- quality
related:
- ai-coding-rules
- decision-single-type-solid-boundaries
owners:
- backend
last_reviewed: 2026-07-21
graph_ready: true
---

# SOLID and C# Type Layout Review

## Scope

The review covers tracked C# production code, runtime tooling, and tests. Generated `bin`, `obj`, and migration artifacts are excluded. SOLID is evaluated by responsibility and dependency boundaries rather than by line count alone.

## Enforced Structure

- Every C# file contains at most one class, record, interface, enum, struct, or delegate.
- The rule includes nested types and test doubles.
- `scripts/validate-csharp-types.ps1` performs the structural check and `scripts/validate-local.ps1` runs it in repository validation.

## SOLID Findings and Changes

| Principle | Review result |
| --- | --- |
| Single Responsibility | Product orchestration no longer owns store authorization, catalog reference validation, and image attachment. Auth0 role synchronization no longer owns Management API token caching. Runtime workflow orchestration no longer contains every scenario and polling implementation. |
| Open/Closed | Runtime scenarios implement `IWorkflowScenarioCheck`; a new scenario is added as a strategy instead of extending a central scenario switch. Product policies can be replaced independently behind focused interfaces. |
| Liskov Substitution | Existing abstractions were checked for implementations that strengthen input requirements or weaken promised results; no concrete violation was found in the reviewed service boundaries. Focused interfaces reduce the behavioral surface substitutes must honor. |
| Interface Segregation | New interfaces expose one capability each: store access validation, product reference validation, image attachment, token supply, gateway operations, workflow probing, and scenario execution. |
| Dependency Inversion | Application orchestration depends on focused interfaces. External HTTP/token and PostgreSQL probe implementations are supplied at composition boundaries instead of being created inside coordinators. |

## Intentionally Cohesive Code

Domain aggregates keep invariant-preserving methods together. Dependency-registration classes remain composition roots. Application services that already represent one bounded use case and depend on repository/publisher interfaces are not split simply because they are long.

## Verification

The repository must pass the C# type-layout validator, solution build and tests, local repository validation, and formatting/diff checks after structural changes.
