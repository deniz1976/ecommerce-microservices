---
id: ai-coding-rules
type: coding-rules
version: 1
status: active
tags:
- coding
- dotnet
- conventions
related:
- ai-architecture
- ai-instructions
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Define repository coding conventions for AI agents and developers.

# Coding Rules

- Code identifiers are English.
- Chat and explanatory documentation may be Turkish or English depending on audience.
- Service business logic stays inside the owning service.
- BuildingBlocks must not contain service-specific domain logic.
- Contracts shared by services belong in `ECommerce.BuildingBlocks.Contracts`.
- Persistence implementations stay in Infrastructure projects.
- Application projects define use cases and interfaces.
- Domain projects define entities and domain state.
- API projects define HTTP endpoints and host startup.
- `Program.cs` files are composition roots only: register dependencies, compose middleware, map endpoints, and start the host.
- Every C# source file declares at most one class, record, interface, enum, struct, or delegate, including test doubles and nested helper types. The file name matches the declared type whenever language or framework conventions permit it.
- Do not declare classes, records, interfaces, or reusable local functions in `Program.cs`; place them in responsibility-based files and folders.
- Repeated host registration and middleware behavior belongs in focused extension classes.
- Application services use `ILogger<T>` for operational logging. Command-line tools report success or failure through process exit codes and must not emit routine `Console` output.
- Apply SOLID by responsibility and change boundary, not by file length: orchestration depends on focused interfaces, external integrations own their protocol/lifecycle concerns, and adding a strategy should not require editing an unrelated strategy.
- Keep domain aggregates cohesive. Do not split invariant-preserving behavior merely to reduce line count.
- Prefer constructor injection over constructing infrastructure collaborators inside application services.

# Testing Rules

- Add focused unit tests for domain/application changes.
- Test doubles follow the same one-type-per-file rule as production code and live in focused support files.
- Use contract tests for shared message contracts.
- Use `tools/ECommerce.RuntimeChecks` for runtime workflow verification.
- `scripts/validate-csharp-types.ps1` enforces the one-type-per-file rule and is part of `scripts/validate-local.ps1`.

# Configuration Rules

- No real secrets in tracked files.
- Runtime secrets come from Infisical or process environment.
- `.env.example` may list keys only.

# Documentation Rules

- Update `/docs/ai` when architecture changes.
- Update service `AI.md` when service responsibilities, APIs, events, or database ownership changes.
- Keep YAML front matter valid.

# TODO

- Add analyzer rules beyond current project defaults.
